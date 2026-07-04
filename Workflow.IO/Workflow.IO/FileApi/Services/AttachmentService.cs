using FileApi.Clients;
using FileApi.Model.Domain.Entities;
using FileApi.Model.Dto;
using FileApi.Repositories;
using Microsoft.AspNetCore.Http;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.Exceptions;
using Workflow.IO.Shared.IntegrationEvents;
using Workflow.IO.Shared.Middleware;
using Workflow.IO.Shared.Persistence;

namespace FileApi.Services
{
    public class AttachmentService : IAttachmentService
    {
        private const long MaxFileSizeInBytes = 25 * 1024 * 1024;

        private readonly IFileRepository _fileRepository;

        private readonly IFileStorageService _fileStorageService;

        private readonly ITaskAccessClient _taskAccessClient;

        private readonly IIntegrationEventPublisher _eventPublisher;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AttachmentService(
            IFileRepository fileRepository,
            IFileStorageService fileStorageService,
            ITaskAccessClient taskAccessClient,
            IIntegrationEventPublisher eventPublisher,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _fileRepository = fileRepository;

            _fileStorageService = fileStorageService;

            _taskAccessClient = taskAccessClient;

            _eventPublisher = eventPublisher;

            _unitOfWork = unitOfWork;

            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<FileAttachmentResponseDto> UploadAsync(
            Guid taskId,
            Guid uploadedById,
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            var task =
                await EnsureTaskAccessAsync(taskId);

            if (file.Length <= 0)
            {
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        ["file"] = ["File is required"]
                    });
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                throw new ValidationException(
                    new Dictionary<string, string[]>
                    {
                        ["file"] =
                        [
                            "File size cannot exceed 25 MB"
                        ]
                    });
            }

            var storedFile =
                await _fileStorageService.SaveAsync(
                    taskId,
                    file,
                    cancellationToken);

            var attachment =
                new FileAttachment(
                    taskId,
                    uploadedById,
                    Path.GetFileName(file.FileName),
                    storedFile.StoredFileName,
                    file.ContentType,
                    file.Length,
                    storedFile.StoragePath);

            await _fileRepository.CreateAsync(attachment);

            await PublishAttachmentEventAsync(
                "AttachmentAdded",
                attachment,
                uploadedById,
                task.AssigneeId == uploadedById
                    ? null
                    : task.AssigneeId,
                $"Attachment '{attachment.OriginalFileName}' was added");

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Map(attachment);
        }

        public async Task<IEnumerable<FileAttachmentResponseDto>>
            GetTaskAttachmentsAsync(Guid taskId)
        {
            await EnsureTaskAccessAsync(taskId);

            var attachments =
                await _fileRepository
                    .GetTaskAttachmentsAsync(taskId);

            return attachments.Select(Map);
        }

        public async Task<AttachmentDownloadResult?> DownloadAsync(
            Guid fileAttachmentId)
        {
            var attachment =
                await _fileRepository
                    .GetByIdAsync(fileAttachmentId);

            if (attachment == null ||
                attachment.IsDeleted)
            {
                return null;
            }

            await EnsureTaskAccessAsync(attachment.TaskId);

            return new AttachmentDownloadResult
            {
                Content =
                    _fileStorageService.OpenRead(
                        attachment.StoragePath),
                ContentType = attachment.ContentType,
                FileName = attachment.OriginalFileName
            };
        }

        public async Task<bool> DeleteAsync(
            Guid fileAttachmentId,
            Guid userId)
        {
            var attachment =
                await _fileRepository
                    .GetByIdAsync(fileAttachmentId);

            if (attachment == null ||
                attachment.IsDeleted)
            {
                return false;
            }

            await EnsureTaskAccessAsync(attachment.TaskId);

            attachment.SoftDelete();

            await PublishAttachmentEventAsync(
                "AttachmentDeleted",
                attachment,
                userId,
                null,
                $"Attachment '{attachment.OriginalFileName}' was deleted");

            await _unitOfWork.SaveChangesAsync();

            _fileStorageService.Delete(attachment.StoragePath);

            return true;
        }

        private async Task<TaskAccessDto> EnsureTaskAccessAsync(
            Guid taskId)
        {
            var task =
                await _taskAccessClient
                    .GetAccessibleTaskAsync(taskId);

            if (task == null)
            {
                throw new ForbiddenException(
                    "User cannot access files for this task");
            }

            return task;
        }

        private async Task PublishAttachmentEventAsync(
            string eventType,
            FileAttachment attachment,
            Guid actorId,
            Guid? recipientId,
            string description)
        {
            await _eventPublisher.PublishAsync(
                new IntegrationEventRequest
                {
                    EventId = Guid.NewGuid(),
                    CorrelationId = GetCorrelationId(),
                    EventType = eventType,
                    EntityType = "Attachment",
                    EntityId = attachment.FileAttachmentId,
                    ActorId = actorId,
                    RecipientId = recipientId,
                    Description = description,
                    PayloadJson =
                        $$"""
                        {
                          "fileAttachmentId": "{{attachment.FileAttachmentId}}",
                          "taskId": "{{attachment.TaskId}}",
                          "fileName": "{{attachment.OriginalFileName}}"
                        }
                        """
                });
        }

        private string? GetCorrelationId() =>
            _httpContextAccessor.HttpContext?
                .Items[CorrelationIdMiddleware.HeaderName]
                ?.ToString();

        private static FileAttachmentResponseDto Map(
            FileAttachment attachment)
        {
            return new FileAttachmentResponseDto
            {
                FileAttachmentId = attachment.FileAttachmentId,
                TaskId = attachment.TaskId,
                UploadedById = attachment.UploadedById,
                OriginalFileName = attachment.OriginalFileName,
                ContentType = attachment.ContentType,
                SizeInBytes = attachment.SizeInBytes,
                CreatedAt = attachment.CreatedAt
            };
        }
    }
}
