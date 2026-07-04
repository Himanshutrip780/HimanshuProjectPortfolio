using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationApi.Data;
using NotificationApi.Model.Domain.Entities;
using System.Security.Claims;
using Workflow.IO.Shared.Contracts;

namespace NotificationApi.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationDbContext _context;

        public NotificationController(NotificationDbContext context)
        {
            _context = context;
        }

        [HttpPost("events")]
        public async Task<IActionResult> ConsumeEvent(
            [FromBody] IntegrationEventRequest request)
        {
            var notification = new Notification(
                request.RecipientId,
                request.EventType,
                request.EntityType,
                request.EntityId,
                request.Description ??
                    $"{request.EventType} occurred for {request.EntityType}");

            await _context.Notifications.AddAsync(notification);

            await _context.SaveChangesAsync();

            return Ok(
                ApiResponse<object>.Ok(
                    new { notification.NotificationId },
                    "Notification event consumed"));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();

            var query =
                _context.Notifications
                    .AsNoTracking()
                    .Where(x => x.RecipientId == userId);

            if (unreadOnly)
            {
                query = query.Where(x => !x.IsRead);
            }

            var notifications =
                await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(100)
                    .ToListAsync();

            return Ok(
                ApiResponse<IEnumerable<Notification>>.Ok(notifications));
        }

        [HttpGet("me/unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();

            var count =
                await _context.Notifications
                    .CountAsync(x =>
                        x.RecipientId == userId &&
                        !x.IsRead);

            return Ok(
                ApiResponse<object>.Ok(
                    new { count }));
        }

        [HttpPatch("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var userId = GetCurrentUserId();

            var notification =
                await _context.Notifications
                    .FirstOrDefaultAsync(x =>
                        x.NotificationId == notificationId &&
                        x.RecipientId == userId);

            if (notification == null)
            {
                return NotFound();
            }

            notification.MarkAsRead();

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { notificationId }));
        }

        [HttpPatch("me/read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();

            var notifications =
                await _context.Notifications
                    .Where(x =>
                        x.RecipientId == userId &&
                        !x.IsRead)
                    .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.MarkAsRead();
            }

            await _context.SaveChangesAsync();

            return Ok(
                ApiResponse<object>.Ok(
                    new { updated = notifications.Count }));
        }

        [HttpGet("users/{userId:guid}")]
        public async Task<IActionResult> GetUserNotifications(
            Guid userId)
        {
            EnsureSelfOrAdmin(userId);

            var notifications =
                await _context.Notifications
                    .AsNoTracking()
                    .Where(x => x.RecipientId == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .Take(100)
                    .ToListAsync();

            return Ok(
                ApiResponse<IEnumerable<Notification>>.Ok(notifications));
        }

        private Guid GetCurrentUserId()
        {
            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(claim))
            {
                throw new UnauthorizedAccessException();
            }

            return Guid.Parse(claim);
        }

        private void EnsureSelfOrAdmin(Guid userId)
        {
            var current =
                GetCurrentUserId();

            var isAdmin =
                User.IsInRole("Admin");

            if (current != userId && !isAdmin)
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
