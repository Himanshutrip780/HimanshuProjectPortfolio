using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ATS.Application.Common.Interfaces
{
    public interface ICandidateSearchService
    {
        Task IndexCandidateAsync(Guid candidateId, string content, CancellationToken cancellationToken = default);
        Task<List<Guid>> SearchCandidatesAsync(Guid companyId, string searchTerm, CancellationToken cancellationToken = default);
    }
}
