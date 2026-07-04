using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Reports;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize]
    public class DashboardController : ApiControllerBase
    {
        [HttpGet("metrics")]
        public async Task<ActionResult<Result<DashboardMetricsDto>>> GetMetrics()
        {
            var result = await Mediator.Send(new GetDashboardMetricsQuery(CompanyId));
            return Ok(result);
        }
    }
}
