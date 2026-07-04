using System.Collections.Generic;

namespace ATS.Application.DTOs.AI
{
    public class WorkExperienceDto
    {
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Duration { get; set; }
        public string Responsibilities { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class EducationDto
    {
        public string Degree { get; set; }
        public string Specialization { get; set; }
        public string University { get; set; }
        public string College { get; set; }
        public string GraduationYear { get; set; }
        public string CGPA { get; set; }
        public string Percentage { get; set; }
    }

    public class ProjectDto
    {
        public string ProjectName { get; set; }
        public List<string> TechnologiesUsed { get; set; } = new List<string>();
        public string Role { get; set; }
        public string ProjectDescription { get; set; }
    }

    public class SkillsCategoryDto
    {
        public List<string> Frontend { get; set; } = new List<string>();
        public List<string> Backend { get; set; } = new List<string>();
        public List<string> Cloud { get; set; } = new List<string>();
        public List<string> Database { get; set; } = new List<string>();
        public List<string> Data { get; set; } = new List<string>();
    }

    public class ConfidenceScoresDto
    {
        public decimal Name { get; set; }
        public decimal Email { get; set; }
        public decimal Phone { get; set; }
        public decimal Skills { get; set; }
        public decimal CurrentCompany { get; set; }
        public decimal CurrentRole { get; set; }
        public decimal Experience { get; set; }
        public decimal Education { get; set; }
        public decimal Projects { get; set; }
    }

    public class ResumeParsingResultDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> Skills { get; set; } = new List<string>();
        public string Education { get; set; }
        public string Experience { get; set; }
        public decimal ConfidenceScore { get; set; }
        public string CurrentTitle { get; set; }
        public string YearsOfExperience { get; set; }
        public string RawText { get; set; }

        // Structured Extraction Redesign
        public string Location { get; set; }
        public string LinkedInUrl { get; set; }
        public string GitHubUrl { get; set; }
        public string PortfolioUrl { get; set; }
        public string Summary { get; set; }
        public List<WorkExperienceDto> WorkExperiences { get; set; } = new List<WorkExperienceDto>();
        public SkillsCategoryDto SkillsCategory { get; set; } = new SkillsCategoryDto();
        public List<EducationDto> Educations { get; set; } = new List<EducationDto>();
        public List<string> Certifications { get; set; } = new List<string>();
        public List<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
        public ConfidenceScoresDto ConfidenceScores { get; set; } = new ConfidenceScoresDto();
        public bool NeedsReview { get; set; }
        public List<string> ReviewReasons { get; set; } = new List<string>();
        public string ParserVersion { get; set; } = "v2.0.0";
        public string Timestamp { get; set; }
    }

    public class AIScoringResultDto
    {
        public int MatchScore { get; set; }
        public decimal SkillMatchPercentage { get; set; }
        public decimal ExperienceMatchPercentage { get; set; }
        public decimal EducationMatchPercentage { get; set; }
        public List<string> MissingSkills { get; set; } = new List<string>();
        public List<string> Strengths { get; set; } = new List<string>();
        public List<string> Weaknesses { get; set; } = new List<string>();
        public string Recommendation { get; set; } // Strong Fit, Moderate Fit, Weak Fit
        public string AISummary { get; set; }
    }

    public class InterviewQuestionsDto
    {
        public List<string> TechnicalQuestions { get; set; } = new List<string>();
        public List<string> BehavioralQuestions { get; set; } = new List<string>();
        public List<string> FollowUpQuestions { get; set; } = new List<string>();
    }
}
