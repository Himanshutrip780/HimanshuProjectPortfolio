using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class CandidateSkill : BaseEntity
    {
        public Guid CandidateId { get; set; }
        public Candidate Candidate { get; set; }
        public string Name { get; set; }
    }
}
