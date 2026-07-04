using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Companies;
using ATS.Application.DTOs.Companies;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter,HiringManager,Interviewer")]
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ApiControllerBase
    {
        [HttpGet("current")]
        public async Task<ActionResult<Result<CompanyDto>>> GetCurrentCompany()
        {
            var result = await Mediator.Send(new GetCompanyQuery(CompanyId));
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPut("current")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<Result>> UpdateCurrentCompany([FromBody] UpdateCompanyModel model)
        {
            var result = await Mediator.Send(new UpdateCompanyCommand
            {
                CompanyId = CompanyId,
                Name = model.Name,
                Domain = model.Domain,
                LogoUrl = model.LogoUrl,
                PrimaryColor = model.PrimaryColor,
                FontFamily = model.FontFamily,
                CustomCss = model.CustomCss,
                SsoEnabled = model.SsoEnabled,
                SsoProvider = model.SsoProvider,
                SsoRedirectUrl = model.SsoRedirectUrl,
                SsoIssuer = model.SsoIssuer,
                SsoClientId = model.SsoClientId
            });

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("public/{companyId}")]
        [AllowAnonymous]
        public async Task<ActionResult<Result<CompanyBrandingDto>>> GetPublicCompanyBranding(Guid companyId)
        {
            var result = await Mediator.Send(new GetCompanyBrandingQuery(companyId));
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }

    public class UpdateCompanyModel
    {
        public string Name { get; set; }
        public string Domain { get; set; }

        // Branding
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; }
        public string? FontFamily { get; set; }
        public string? CustomCss { get; set; }

        // SSO
        public bool SsoEnabled { get; set; }
        public string? SsoProvider { get; set; }
        public string? SsoRedirectUrl { get; set; }
        public string? SsoIssuer { get; set; }
        public string? SsoClientId { get; set; }
    }
}
