using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Notifications;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize]
    public class NotificationsController : ApiControllerBase
    {
        private Guid CurrentUserId
        {
            get
            {
                var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(val, out var guid) ? guid : Guid.Empty;
            }
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<NotificationDto>>>> GetNotifications()
        {
            var result = await Mediator.Send(new GetNotificationsQuery(CurrentUserId));
            return Ok(result);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<Result>> MarkAsRead(Guid id)
        {
            var result = await Mediator.Send(new MarkNotificationAsReadCommand(id, CurrentUserId));
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("read-all")]
        public async Task<ActionResult<Result>> MarkAllAsRead()
        {
            var result = await Mediator.Send(new MarkAllNotificationsAsReadCommand(CurrentUserId));
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
