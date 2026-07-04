using System.Security.Claims;
using FileApi.Model.Dto;
using FileApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflow.IO.Shared.Contracts;

namespace FileApi.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;

        public FileController(
            IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpPost("tasks/{taskId:guid}/attachments")]
        [RequestSizeLimit(25 * 1024 * 1024)]
        public async Task<IActionResult> UploadAttachment(
            Guid taskId,
            IFormFile file,
            CancellationToken cancellationToken)
        {
            var attachment =
                await _attachmentService.UploadAsync(
                    taskId,
                    GetCurrentUserId(),
                    file,
                    cancellationToken);

            return Ok(
                ApiResponse<FileAttachmentResponseDto>.Ok(
                    attachment,
                    "Attachment uploaded successfully"));
        }

        [HttpGet("tasks/{taskId:guid}/attachments")]
        public async Task<IActionResult> GetTaskAttachments(
            Guid taskId)
        {
            var attachments =
                await _attachmentService
                    .GetTaskAttachmentsAsync(taskId);

            return Ok(
                ApiResponse<IEnumerable<FileAttachmentResponseDto>>.Ok(
                    attachments));
        }

        [HttpGet("attachments/{fileAttachmentId:guid}/download")]
        public async Task<IActionResult> DownloadAttachment(
            Guid fileAttachmentId)
        {
            var result =
                await _attachmentService
                    .DownloadAsync(fileAttachmentId);

            if (result == null)
            {
                return NotFound();
            }

            return File(
                result.Content,
                result.ContentType,
                result.FileName);
        }

        [HttpDelete("attachments/{fileAttachmentId:guid}")]
        public async Task<IActionResult> DeleteAttachment(
            Guid fileAttachmentId)
        {
            var deleted =
                await _attachmentService.DeleteAsync(
                    fileAttachmentId,
                    GetCurrentUserId());

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException();
            }

            return Guid.Parse(userIdClaim);
        }
    }
}
