using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Shared.Models;

namespace ATS.Application.Features.AuditLogs
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string UserEmail { get; set; }
        public string TableName { get; set; }
        public string Action { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public record GetAuditLogsQuery : IRequest<Result<PaginatedList<AuditLogDto>>>
    {
        public int PageIndex { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? TableName { get; init; }
        public string? Action { get; init; }
    }

    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Result<PaginatedList<AuditLogDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAuditLogsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<PaginatedList<AuditLogDto>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrEmpty(request.TableName))
            {
                query = query.Where(a => a.TableName.Contains(request.TableName));
            }

            if (!string.IsNullOrEmpty(request.Action))
            {
                query = query.Where(a => a.Action.Contains(request.Action));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userIds = items.Where(i => i.UserId.HasValue).Select(i => i.UserId.Value).Distinct().ToList();
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken);

            var dtos = items.Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.UserId.HasValue && users.TryGetValue(a.UserId.Value, out var email) ? email : "System / Anonymous",
                TableName = a.TableName,
                Action = a.Action,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                Timestamp = a.Timestamp
            }).ToList();

            var paginatedList = new PaginatedList<AuditLogDto>(dtos, totalCount, request.PageIndex, request.PageSize);
            return Result<PaginatedList<AuditLogDto>>.Success(paginatedList);
        }
    }
}
