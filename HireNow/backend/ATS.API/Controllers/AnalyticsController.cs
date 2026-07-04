using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Reports;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter")]
    public class AnalyticsController : ApiControllerBase
    {
        [HttpGet("report")]
        public async Task<ActionResult<Result<AnalyticsReportDto>>> GetReport()
        {
            var result = await Mediator.Send(new GetAnalyticsReportQuery(CompanyId));
            return Ok(result);
        }
    }
}
