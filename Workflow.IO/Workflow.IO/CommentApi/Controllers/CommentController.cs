using System.Security.Claims;
using CommentApi.Model.Dto;
using CommentApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workflow.IO.Shared.Contracts;

namespace CommentApi.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(
            ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost("tasks/{taskId:guid}/comments")]
        public async Task<IActionResult> CreateComment(
            Guid taskId,
            [FromBody] CreateCommentRequestDto request)
        {
            var comment =
                await _commentService.CreateAsync(
                    taskId,
                    GetCurrentUserId(),
                    request);

            return Ok(
                ApiResponse<CommentResponseDto>.Ok(
                    comment,
                    "Comment created successfully"));
        }

        [HttpGet("tasks/{taskId:guid}/comments")]
        public async Task<IActionResult> GetTaskComments(
            Guid taskId)
        {
            var comments =
                await _commentService
                    .GetTaskCommentsAsync(taskId);

            return Ok(
                ApiResponse<IEnumerable<CommentResponseDto>>.Ok(
                    comments));
        }

        [HttpPut("comments/{commentId:guid}")]
        public async Task<IActionResult> UpdateComment(
            Guid commentId,
            [FromBody] UpdateCommentRequestDto request)
        {
            var comment =
                await _commentService.UpdateAsync(
                    commentId,
                    GetCurrentUserId(),
                    request);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(
                ApiResponse<CommentResponseDto>.Ok(
                    comment,
                    "Comment updated successfully"));
        }

        [HttpDelete("comments/{commentId:guid}")]
        public async Task<IActionResult> DeleteComment(
            Guid commentId)
        {
            var deleted =
                await _commentService.DeleteAsync(
                    commentId,
                    GetCurrentUserId());

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpPost("comments/{commentId:guid}/reactions")]
        public async Task<IActionResult> AddReaction(
            Guid commentId,
            [FromBody] CommentReactionRequestDto request)
        {
            var comment = await _commentService.AddReactionAsync(
                commentId,
                GetCurrentUserId(),
                request.Emoji);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<CommentResponseDto>.Ok(comment, "Reaction added successfully"));
        }

        [HttpDelete("comments/{commentId:guid}/reactions")]
        public async Task<IActionResult> RemoveReaction(
            Guid commentId,
            [FromQuery] string emoji)
        {
            var comment = await _commentService.RemoveReactionAsync(
                commentId,
                GetCurrentUserId(),
                emoji);

            if (comment == null)
            {
                return NotFound();
            }

            return Ok(ApiResponse<CommentResponseDto>.Ok(comment, "Reaction removed successfully"));
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
