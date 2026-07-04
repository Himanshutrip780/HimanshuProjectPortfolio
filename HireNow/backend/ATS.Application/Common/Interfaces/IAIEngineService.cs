using System;
using System.Threading.Tasks;
using ATS.Application.DTOs.AI;

namespace ATS.Application.Common.Interfaces
{
    public interface IAIEngineService
    {
        Task<ResumeParsingResultDto> ParseResumeAsync(byte[] fileData, string fileExtension, string fileName = null);
        Task<AIScoringResultDto> ScoreCandidateAsync(string resumeText, string jobDescription, Guid? candidateId = null, Guid? jobId = null);
        Task<InterviewQuestionsDto> SuggestInterviewQuestionsAsync(string resumeText, string jobDescription, Guid? candidateId = null, Guid? jobId = null);
    }
}
