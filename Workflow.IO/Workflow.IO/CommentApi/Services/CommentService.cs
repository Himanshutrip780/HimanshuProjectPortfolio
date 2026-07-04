using AutoMapper;
using CommentApi.Clients;
using CommentApi.Model.Domain.Entities;
using CommentApi.Model.Dto;
using CommentApi.Repositories;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.Exceptions;
using Workflow.IO.Shared.IntegrationEvents;
using Workflow.IO.Shared.Middleware;
using Workflow.IO.Shared.Persistence;
using Microsoft.AspNetCore.Http;

namespace CommentApi.Services
{
    public class CommentService
        : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        private readonly IMapper _mapper;

        private readonly IIntegrationEventPublisher _eventPublisher;

        private readonly ITaskAccessClient _taskAccessClient;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommentService(
            ICommentRepository commentRepository,
            IMapper mapper,
            IIntegrationEventPublisher eventPublisher,
            ITaskAccessClient taskAccessClient,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _commentRepository = commentRepository;

            _mapper = mapper;

            _eventPublisher = eventPublisher;

            _taskAccessClient = taskAccessClient;

            _unitOfWork = unitOfWork;

            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CommentResponseDto> CreateAsync(
            Guid taskId,
            Guid authorId,
            CreateCommentRequestDto request)
        {
            var task =
                await EnsureTaskAccessAsync(taskId);

            if (request.ParentCommentId.HasValue)
            {
                var parent =
                    await _commentRepository
                        .GetByIdAsync(
                            request.ParentCommentId.Value);

                if (parent == null ||
                    parent.TaskId != taskId ||
                    parent.IsDeleted)
                {
                    throw new NotFoundException(
                        "Parent comment was not found");
                }
            }

            var comment = new Comment(
                taskId,
                authorId,
                request.Body,
                request.ParentCommentId);

            await _commentRepository.CreateAsync(comment);

            await SaveMentionsAsync(
                comment.CommentId,
                request.MentionedUserIds);

            await PublishCommentEventAsync(
                "CommentAdded",
                comment,
                authorId,
                task.AssigneeId == authorId
                    ? null
                    : task.AssigneeId,
                "A comment was added");

            await PublishMentionEventsAsync(
                comment,
                authorId,
                request.MentionedUserIds);

            await _unitOfWork.SaveChangesAsync();

            return await MapCommentAsync(comment);
        }

        public async Task<IEnumerable<CommentResponseDto>> GetTaskCommentsAsync(
            Guid taskId)
        {
            await EnsureTaskAccessAsync(taskId);

            var comments =
                await _commentRepository
                    .GetByTaskIdAsync(taskId);

            var response = new List<CommentResponseDto>();

            foreach (var comment in comments)
            {
                response.Add(
                    await MapCommentAsync(comment));
            }

            return response;
        }

        public async Task<CommentResponseDto?> UpdateAsync(
            Guid commentId,
            Guid userId,
            UpdateCommentRequestDto request)
        {
            var comment =
                await _commentRepository
                    .GetByIdAsync(commentId);

            if (comment == null || comment.IsDeleted)
            {
                return null;
            }

            EnsureAuthor(comment, userId);

            await EnsureTaskAccessAsync(comment.TaskId);

            comment.Update(request.Body);

            await _commentRepository.ReplaceMentionsAsync(
                comment.CommentId,
                CreateMentions(
                    comment.CommentId,
                    request.MentionedUserIds));

            await PublishCommentEventAsync(
                "CommentUpdated",
                comment,
                userId,
                null,
                "A comment was updated");

            await PublishMentionEventsAsync(
                comment,
                userId,
                request.MentionedUserIds);

            await _unitOfWork.SaveChangesAsync();

            return await MapCommentAsync(comment);
        }

        public async Task<bool> DeleteAsync(
            Guid commentId,
            Guid userId)
        {
            var comment =
                await _commentRepository
                    .GetByIdAsync(commentId);

            if (comment == null || comment.IsDeleted)
            {
                return false;
            }

            EnsureAuthor(comment, userId);

            await EnsureTaskAccessAsync(comment.TaskId);

            comment.SoftDelete();

            await PublishCommentEventAsync(
                "CommentDeleted",
                comment,
                userId,
                null,
                "A comment was deleted");

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<CommentResponseDto?> AddReactionAsync(
            Guid commentId,
            Guid userId,
            string emoji)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.IsDeleted)
            {
                return null;
            }

            await EnsureTaskAccessAsync(comment.TaskId);

            var existingReaction = comment.Reactions
                .FirstOrDefault(r => r.UserId == userId && r.Emoji == emoji);

            if (existingReaction == null)
            {
                var reaction = new CommentReaction(commentId, userId, emoji);
                comment.Reactions.Add(reaction);
                await _unitOfWork.SaveChangesAsync();
            }

            return await MapCommentAsync(comment);
        }

        public async Task<CommentResponseDto?> RemoveReactionAsync(
            Guid commentId,
            Guid userId,
            string emoji)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.IsDeleted)
            {
                return null;
            }

            await EnsureTaskAccessAsync(comment.TaskId);

            var existingReaction = comment.Reactions
                .FirstOrDefault(r => r.UserId == userId && r.Emoji == emoji);

            if (existingReaction != null)
            {
                comment.Reactions.Remove(existingReaction);
                await _unitOfWork.SaveChangesAsync();
            }

            return await MapCommentAsync(comment);
        }

        private static void EnsureAuthor(
            Comment comment,
            Guid userId)
        {
            if (comment.AuthorId != userId)
            {
                throw new ForbiddenException(
                    "Only the comment author can modify this comment");
            }
        }

        private async Task PublishCommentEventAsync(
            string eventType,
            Comment comment,
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
                    EntityType = "Comment",
                    EntityId = comment.CommentId,
                    ActorId = actorId,
                    RecipientId = recipientId,
                    Description = description,
                    PayloadJson =
                        $$"""
                        {
                          "commentId": "{{comment.CommentId}}",
                          "taskId": "{{comment.TaskId}}",
                          "parentCommentId": "{{comment.ParentCommentId}}"
                        }
                        """
                });
        }

        private async Task PublishMentionEventsAsync(
            Comment comment,
            Guid actorId,
            IEnumerable<Guid> mentionedUserIds)
        {
            foreach (var mentionedUserId in mentionedUserIds.Distinct())
            {
                if (mentionedUserId == actorId)
                {
                    continue;
                }

                await _eventPublisher.PublishAsync(
                    new IntegrationEventRequest
                    {
                        EventId = Guid.NewGuid(),
                        CorrelationId = GetCorrelationId(),
                        EventType = "UserMentioned",
                        EntityType = "Comment",
                        EntityId = comment.CommentId,
                        ActorId = actorId,
                        RecipientId = mentionedUserId,
                        Description = "You were mentioned in a comment",
                        PayloadJson =
                            $$"""
                            {
                              "commentId": "{{comment.CommentId}}",
                              "taskId": "{{comment.TaskId}}"
                            }
                            """
                    });
            }
        }

        private async Task SaveMentionsAsync(
            Guid commentId,
            IEnumerable<Guid> mentionedUserIds)
        {
            await _commentRepository.AddMentionsAsync(
                CreateMentions(
                    commentId,
                    mentionedUserIds));
        }

        private static IEnumerable<CommentMention> CreateMentions(
            Guid commentId,
            IEnumerable<Guid> mentionedUserIds)
        {
            return mentionedUserIds
                .Distinct()
                .Select(userId =>
                    new CommentMention(
                        commentId,
                        userId));
        }

        private async Task<CommentResponseDto> MapCommentAsync(
            Comment comment)
        {
            var response =
                _mapper.Map<CommentResponseDto>(comment);

            var mentions =
                await _commentRepository
                    .GetMentionsAsync(comment.CommentId);

            response.MentionedUserIds =
                mentions.Select(x => x.MentionedUserId);

            return response;
        }

        private string? GetCorrelationId() =>
            _httpContextAccessor.HttpContext?
                .Items[CorrelationIdMiddleware.HeaderName]
                ?.ToString();

        private async Task<TaskAccessDto> EnsureTaskAccessAsync(
            Guid taskId)
        {
            var task =
                await _taskAccessClient
                    .GetAccessibleTaskAsync(taskId);

            if (task == null)
            {
                throw new ForbiddenException(
                    "User cannot access comments for this task");
            }

            return task;
        }
    }
}
