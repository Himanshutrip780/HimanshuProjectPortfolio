using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Domain.Entities;

namespace ATS.Infrastructure.Services
{
    public class CandidateSearchService : ICandidateSearchService
    {
        private readonly IApplicationDbContext _context;

        public CandidateSearchService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task IndexCandidateAsync(Guid candidateId, string content, CancellationToken cancellationToken = default)
        {
            var candidate = await _context.Candidates
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == candidateId, cancellationToken);

            if (candidate == null)
            {
                return;
            }

            var existingIndex = await _context.CandidateSearchIndices
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.CandidateId == candidateId, cancellationToken);

            if (existingIndex == null)
            {
                var newIndex = new CandidateSearchIndex
                {
                    CandidateId = candidateId,
                    CompanyId = candidate.CompanyId,
                    SearchableText = NormalizeContent(content)
                };
                _context.CandidateSearchIndices.Add(newIndex);
            }
            else
            {
                existingIndex.SearchableText = NormalizeContent(content);
                existingIndex.IsDeleted = false; // Ensure it is active if it was previously soft deleted
                _context.CandidateSearchIndices.Update(existingIndex);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Guid>> SearchCandidatesAsync(Guid companyId, string searchTerm, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Guid>();
            }

            var searchTerms = searchTerm.ToLower()
                .Split(new[] { ' ', ',', ';', '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (searchTerms.Length == 0)
            {
                return new List<Guid>();
            }

            var query = _context.CandidateSearchIndices
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId && !x.IsDeleted);

            foreach (var term in searchTerms)
            {
                query = query.Where(x => x.SearchableText.ToLower().Contains(term));
            }

            return await query
                .Select(x => x.CandidateId)
                .ToListAsync(cancellationToken);
        }

        private static string NormalizeContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;
            return content.Trim();
        }
    }
}
