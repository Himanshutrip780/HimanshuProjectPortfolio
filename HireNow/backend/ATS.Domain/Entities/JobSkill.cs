using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class JobSkill : BaseEntity
    {
        public Guid JobId { get; set; }
        public Job Job { get; set; }
        public string Name { get; set; }
    }
}
