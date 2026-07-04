using System;
using System.ComponentModel.DataAnnotations;

namespace UserApi.Model.Domian.Entities
{
    public class EmailVerificationAuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(256)]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? IPAddress { get; set; }

        [MaxLength(512)]
        public string? UserAgent { get; set; }

        public string? Details { get; set; }
    }
}
