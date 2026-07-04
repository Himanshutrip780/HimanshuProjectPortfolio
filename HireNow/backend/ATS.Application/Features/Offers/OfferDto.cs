using System;
using ATS.Domain.Enums;

namespace ATS.Application.Features.Offers
{
    public class OfferDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public string CandidateName { get; set; }
        public string JobTitle { get; set; }
        public decimal Salary { get; set; }
        public DateTime StartDate { get; set; }
        public OfferStatus Status { get; set; }
        public string OfferLetterPath { get; set; }
        public string OfferLetterContent { get; set; }
        public string ESignatureDetails { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
