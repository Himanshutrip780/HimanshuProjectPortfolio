using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class CandidateNote : BaseEntity
    {
        public Guid CandidateId { get; set; }
        public Candidate Candidate { get; set; }

        public Guid? ApplicationId { get; set; }
        public Application Application { get; set; }

        public string Text { get; set; }
        public string AuthorName { get; set; }
    }
}
