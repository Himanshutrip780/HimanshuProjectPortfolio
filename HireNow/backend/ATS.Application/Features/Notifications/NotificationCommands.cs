using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Shared.Models;

namespace ATS.Application.Features.Notifications
{
    public record MarkNotificationAsReadCommand(Guid Id, Guid UserId) : IRequest<Result>;

    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public MarkNotificationAsReadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == request.UserId, cancellationToken);

            if (notification == null)
            {
                return Result.Failure("Notification not found.");
            }

            notification.IsRead = true;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<Result>;

    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public MarkAllNotificationsAsReadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == request.UserId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var n in notifications)
            {
                n.IsRead = true;
                _context.Notifications.Update(n);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
