using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class TenantSmtpSettings : BaseEntity, IMultiTenant
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderAddress { get; set; }
        public string SenderName { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
