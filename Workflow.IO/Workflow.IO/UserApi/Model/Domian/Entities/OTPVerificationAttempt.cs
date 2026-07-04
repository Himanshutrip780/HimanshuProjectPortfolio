using System;
using System.ComponentModel.DataAnnotations;

namespace UserApi.Model.Domian.Entities
{
    public class OTPVerificationAttempt
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid EmailVerificationRequestId { get; set; }

        [Required]
        [MaxLength(256)]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string OtpAttempt { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsSuccessful { get; set; }

        [MaxLength(100)]
        public string? IPAddress { get; set; }

        [MaxLength(512)]
        public string? UserAgent { get; set; }
    }
}
