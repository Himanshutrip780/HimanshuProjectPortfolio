using System;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ATS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender _mediator;

        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

        protected Guid CompanyId
        {
            get
            {
                var claimValue = User.FindFirstValue("CompanyId");
                if (Guid.TryParse(claimValue, out var companyId))
                {
                    return companyId;
                }
                return Guid.Empty;
            }
        }
    }
}
