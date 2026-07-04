using System;
using ATS.Domain.Common;

namespace ATS.Domain.Entities
{
    public class EmailTemplate : BaseEntity, IMultiTenant
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }

        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string TriggerEvent { get; set; } // ApplicationReceived, InterviewScheduled, InterviewReminder, Rejection, OfferSent
    }
}
