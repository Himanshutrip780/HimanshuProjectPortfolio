namespace ATS.Shared.Constants
{
    public static class Stages
    {
        public const string Applied = "Applied";
        public const string Screening = "Screening";
        public const string RecruiterReview = "Recruiter Review";
        public const string HiringManagerReview = "Hiring Manager Review";
        public const string TechnicalInterview = "Technical Interview";
        public const string HRInterview = "HR Interview";
        public const string FinalInterview = "Final Interview";
        public const string Offer = "Offer";
        public const string Hired = "Hired";
        public const string Rejected = "Rejected";

        public static readonly string[] PipelineOrder = 
        {
            Applied,
            Screening,
            RecruiterReview,
            HiringManagerReview,
            TechnicalInterview,
            HRInterview,
            FinalInterview,
            Offer,
            Hired,
            Rejected
        };
    }
}
