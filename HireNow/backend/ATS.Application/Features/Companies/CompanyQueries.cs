using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Application.DTOs.Companies;
using ATS.Shared.Models;

namespace ATS.Application.Features.Companies
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
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

    public record GetCompanyQuery(Guid CompanyId) : IRequest<Result<CompanyDto>>;

    public class GetCompanyQueryHandler : IRequestHandler<GetCompanyQuery, Result<CompanyDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCompanyQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CompanyDto>> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
        {
            var company = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

            if (company == null)
            {
                return Result<CompanyDto>.Failure("Company not found.");
            }

            var dto = new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Domain = company.Domain,
                LogoUrl = company.LogoUrl,
                PrimaryColor = company.PrimaryColor,
                FontFamily = company.FontFamily,
                CustomCss = company.CustomCss,
                SsoEnabled = company.SsoEnabled,
                SsoProvider = company.SsoProvider,
                SsoRedirectUrl = company.SsoRedirectUrl,
                SsoIssuer = company.SsoIssuer,
                SsoClientId = company.SsoClientId
            };

            return Result<CompanyDto>.Success(dto);
        }
    }

    public record GetCompanyBrandingQuery(Guid CompanyId) : IRequest<Result<CompanyBrandingDto>>;

    public class GetCompanyBrandingQueryHandler : IRequestHandler<GetCompanyBrandingQuery, Result<CompanyBrandingDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCompanyBrandingQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<CompanyBrandingDto>> Handle(GetCompanyBrandingQuery request, CancellationToken cancellationToken)
        {
            var company = await _context.Companies
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

            if (company == null)
            {
                return Result<CompanyBrandingDto>.Failure("Company not found.");
            }

            var dto = new CompanyBrandingDto
            {
                CompanyName = company.Name,
                LogoUrl = company.LogoUrl,
                PrimaryColor = company.PrimaryColor,
                FontFamily = company.FontFamily,
                CustomCss = company.CustomCss
            };

            return Result<CompanyBrandingDto>.Success(dto);
        }
    }
}
