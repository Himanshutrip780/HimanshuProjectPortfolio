using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class InterviewQuestionTemplate : BaseEntity
    {
        public string SkillName { get; set; }
        public string Question { get; set; }
        public string Category { get; set; } // Technical, Behavioral, FollowUp
    }
}
