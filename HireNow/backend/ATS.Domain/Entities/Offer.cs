using System;
using ATS.Domain.Common;
using ATS.Domain.Enums;

namespace ATS.Domain.Entities
{
    public class Offer : BaseEntity
    {
        public Guid ApplicationId { get; set; }
        public Application Application { get; set; }

        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
        public OfferStatus Status { get; set; } = OfferStatus.Draft;
        public string OfferLetterPath { get; set; }
        public string OfferLetterContent { get; set; }
        public string ESignatureDetails { get; set; }
    }
}
