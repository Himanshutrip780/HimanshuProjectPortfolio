namespace ATS.Shared.Constants
{
    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Recruiter = "Recruiter";
        public const string HiringManager = "HiringManager";
        public const string Interviewer = "Interviewer";
        public const string Candidate = "Candidate";

        public static readonly string[] All = { SuperAdmin, Recruiter, HiringManager, Interviewer, Candidate };
    }
}
