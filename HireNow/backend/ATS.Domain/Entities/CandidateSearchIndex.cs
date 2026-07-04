using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class CandidateSearchIndex : BaseEntity, IMultiTenant
    {
        public Guid CandidateId { get; set; }
        public Candidate Candidate { get; set; }
        
        public string SearchableText { get; set; }

        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
