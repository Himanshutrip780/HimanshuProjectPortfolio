using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.AuditLogs;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter")]
    public class AuditLogsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Result<PaginatedList<AuditLogDto>>>> Get(
            [FromQuery] string? tableName,
            [FromQuery] string? action,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await Mediator.Send(new GetAuditLogsQuery
            {
                TableName = tableName,
                Action = action,
                PageIndex = pageIndex,
                PageSize = pageSize
            });

            return Ok(result);
        }
    }
}
