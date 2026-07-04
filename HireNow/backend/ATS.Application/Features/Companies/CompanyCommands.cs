using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Shared.Models;

namespace ATS.Application.Features.Companies
{
    public record UpdateCompanyCommand : IRequest<Result>
    {
        public Guid CompanyId { get; init; }
        public string Name { get; init; }
        public string Domain { get; init; }

        // Branding
        public string? LogoUrl { get; init; }
        public string? PrimaryColor { get; init; }
        public string? FontFamily { get; init; }
        public string? CustomCss { get; init; }

        // SSO
        public bool SsoEnabled { get; init; }
        public string? SsoProvider { get; init; }
        public string? SsoRedirectUrl { get; init; }
        public string? SsoIssuer { get; init; }
        public string? SsoClientId { get; init; }
    }

    public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result>
    {
        private readonly IApplicationDbContext _context;

        public UpdateCompanyCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

            if (company == null)
            {
                return Result.Failure("Company not found.");
            }

            company.Name = request.Name;
            company.Domain = request.Domain;

            // Branding
            company.LogoUrl = request.LogoUrl;
            company.PrimaryColor = request.PrimaryColor;
            company.FontFamily = request.FontFamily;
            company.CustomCss = request.CustomCss;

            // SSO
            company.SsoEnabled = request.SsoEnabled;
            company.SsoProvider = request.SsoProvider;
            company.SsoRedirectUrl = request.SsoRedirectUrl;
            company.SsoIssuer = request.SsoIssuer;
            company.SsoClientId = request.SsoClientId;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
