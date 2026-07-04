using System;
using System.ComponentModel.DataAnnotations;

namespace UserApi.Model.Domian.Entities
{
    public class EmailVerificationRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string OtpHash { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string VerificationStatus { get; set; } = "Pending";

        public int AttemptCount { get; set; }

        public int ResendCount { get; set; }

        [MaxLength(100)]
        public string? IPAddress { get; set; }

        [MaxLength(512)]
        public string? UserAgent { get; set; }

        public string? RegistrationPayload { get; set; }

        public DateTime? LastResentAt { get; set; }
    }
}
