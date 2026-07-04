using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
