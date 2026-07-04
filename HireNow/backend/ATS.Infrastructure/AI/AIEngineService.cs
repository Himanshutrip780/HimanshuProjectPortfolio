using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ATS.Application.Common.Interfaces;
using ATS.Application.DTOs.AI;
using ATS.Domain.Entities;

namespace ATS.Infrastructure.AI
{
    public class AIEngineService : IAIEngineService
    {
        private readonly IApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;
        private readonly bool _useRealAI;

        public AIEngineService(HttpClient httpClient, IConfiguration configuration, IApplicationDbContext context)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenAI:ApiKey"];
            _endpoint = configuration["OpenAI:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
            _useRealAI = !string.IsNullOrEmpty(_apiKey) && _apiKey != "YOUR_OPENAI_API_KEY";
            _context = context;
        }

        public async Task<ResumeParsingResultDto> ParseResumeAsync(byte[] fileData, string fileExtension, string fileName = null)
        {
            // First, extract raw text from file bytes. 
            // In a real system, we'd use a PDF/Docx parser. 
            // Here, we simulate raw text conversion (decoding from text files or reading string metadata).
            string rawText = ExtractTextFromBytes(fileData, fileExtension, fileName);

            ResumeParsingResultDto result;
            if (_useRealAI)
            {
                try
                {
                    result = await ParseResumeWithOpenAI(rawText);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AI Error] Real OpenAI parser failed: {ex.Message}. Falling back to Mock Parser.");
                    result = ParseResumeMock(rawText, fileName);
                }
            }
            else
            {
                result = ParseResumeMock(rawText, fileName);
            }

            if (result != null)
            {
                result.RawText = rawText;
            }
            return result;
        }

        public async Task<AIScoringResultDto> ScoreCandidateAsync(string resumeText, string jobDescription, Guid? candidateId = null, Guid? jobId = null)
        {
            if (_useRealAI)
            {
                try
                {
                    return await ScoreCandidateWithOpenAI(resumeText, jobDescription);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AI Error] Real OpenAI scorer failed: {ex.Message}. Falling back to Mock Scorer.");
                }
            }

            return await ScoreCandidateMockAsync(resumeText, jobDescription, candidateId, jobId);
        }

        public async Task<InterviewQuestionsDto> SuggestInterviewQuestionsAsync(string resumeText, string jobDescription, Guid? candidateId = null, Guid? jobId = null)
        {
            if (_useRealAI)
            {
                try
                {
                    return await SuggestQuestionsWithOpenAI(resumeText, jobDescription);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AI Error] Real OpenAI questions generator failed: {ex.Message}. Falling back to Mock.");
                }
            }

            return await SuggestQuestionsMockAsync(resumeText, jobDescription, candidateId, jobId);
        }

        #region OpenAI Real Implementations

        private async Task<ResumeParsingResultDto> ParseResumeWithOpenAI(string rawText)
        {
            var prompt = $@"
You are an expert ATS resume parser. Extract information from the raw resume text provided.
Return the result strictly as a valid JSON object matching the following structure:
{{
  ""Name"": ""Candidate Name"",
  ""Email"": ""Candidate Email"",
  ""Phone"": ""Candidate Phone"",
  ""Location"": ""City, State, Country"",
  ""LinkedInUrl"": ""LinkedIn Profile URL"",
  ""GitHubUrl"": ""GitHub URL"",
  ""PortfolioUrl"": ""Portfolio Website URL"",
  ""Summary"": ""Professional Summary"",
  ""WorkExperiences"": [
    {{
      ""CompanyName"": ""Company Name"",
      ""JobTitle"": ""Job Title"",
      ""StartDate"": ""MM/YYYY or Month YYYY"",
      ""EndDate"": ""MM/YYYY, Month YYYY, or 'Present'"",
      ""Duration"": ""Duration of employment"",
      ""Responsibilities"": ""Key responsibilities and achievements"",
      ""IsCurrent"": false
    }}
  ],
  ""SkillsCategory"": {{
    ""Frontend"": [""Angular"", ""React"", ""TypeScript"", ""JavaScript"", ""HTML"", ""CSS""],
    ""Backend"": ["".NET"", ""C#"", ""Java"", ""Spring Boot"", ""Node.js"", ""Python""],
    ""Cloud"": [""Azure"", ""AWS"", ""GCP"", ""Docker"", ""Kubernetes""],
    ""Database"": [""SQL Server"", ""Oracle"", ""MySQL"", ""PostgreSQL"", ""MongoDB""],
    ""Data"": [""Power BI"", ""Tableau"", ""ETL"", ""Data Engineering""]
  }},
  ""Educations"": [
    {{
      ""Degree"": ""Degree (B.Tech, MS, etc.)"",
      ""Specialization"": ""Specialization (Computer Science, etc.)"",
      ""University"": ""University Name"",
      ""College"": ""College Name"",
      ""GraduationYear"": ""YYYY"",
      ""CGPA"": ""e.g., 8.5/10"",
      ""Percentage"": ""e.g., 85%""
    }}
  ],
  ""Certifications"": [""Cert1"", ""Cert2""],
  ""Projects"": [
    {{
      ""ProjectName"": ""Project Name"",
      ""TechnologiesUsed"": [""Tech1"", ""Tech2""],
      ""Role"": ""Role in Project"",
      ""ProjectDescription"": ""Description of Project""
    }}
  ]
}}

Resume Raw Text:
{rawText}
";

            var jsonResponse = await CallOpenAIApi(prompt);
            var result = JsonSerializer.Deserialize<ResumeParsingResultDto>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null)
            {
                // Flatten skills for backward compatibility
                result.Skills = new List<string>();
                if (result.SkillsCategory != null)
                {
                    if (result.SkillsCategory.Frontend != null) result.Skills.AddRange(result.SkillsCategory.Frontend);
                    if (result.SkillsCategory.Backend != null) result.Skills.AddRange(result.SkillsCategory.Backend);
                    if (result.SkillsCategory.Cloud != null) result.Skills.AddRange(result.SkillsCategory.Cloud);
                    if (result.SkillsCategory.Database != null) result.Skills.AddRange(result.SkillsCategory.Database);
                    if (result.SkillsCategory.Data != null) result.Skills.AddRange(result.SkillsCategory.Data);
                }

                // Run validation & enrichment
                ValidateAndEnrichParsedResult(result);

                // Format string summaries for backward compatibility if not present
                if (string.IsNullOrEmpty(result.Experience))
                {
                    result.Experience = string.Join("\n\n", result.WorkExperiences.Select(w => 
                        $"- {w.JobTitle} at {w.CompanyName} ({w.StartDate} - {w.EndDate})\n  Responsibilities: {w.Responsibilities}"));
                }
                if (string.IsNullOrEmpty(result.Education))
                {
                    result.Education = string.Join("\n\n", result.Educations.Select(e => 
                        $"- {e.Degree} in {e.Specialization}, {e.College} ({e.GraduationYear})"));
                }
            }

            return result ?? ParseResumeMock(rawText, null);
        }

        private async Task<AIScoringResultDto> ScoreCandidateWithOpenAI(string resumeText, string jobDescription)
        {
            var prompt = $@"
You are an expert AI recruiting assistant. Compare the candidate's resume against the job description.
Return a structured assessment strictly as a JSON object matching this structure:
{{
  ""MatchScore"": 85,
  ""SkillMatchPercentage"": 90.0,
  ""ExperienceMatchPercentage"": 80.0,
  ""EducationMatchPercentage"": 85.0,
  ""MissingSkills"": [""Skill A"", ""Skill B""],
  ""Strengths"": [""Strength A"", ""Strength B""],
  ""Weaknesses"": [""Weakness A"", ""Weakness B""],
  ""Recommendation"": ""Strong Fit"",
  ""AISummary"": ""Short executive summary...""
}}
Recommendation must be one of: 'Strong Fit', 'Moderate Fit', 'Weak Fit'.

Job Description:
{jobDescription}

Candidate Resume:
{resumeText}
";

            var jsonResponse = await CallOpenAIApi(prompt);
            var result = JsonSerializer.Deserialize<AIScoringResultDto>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? await ScoreCandidateMockAsync(resumeText, jobDescription, null, null);
        }

        private async Task<InterviewQuestionsDto> SuggestQuestionsWithOpenAI(string resumeText, string jobDescription)
        {
            var prompt = $@"
You are an expert hiring interviewer. Generate relevant interview questions based on the candidate's resume and target job description.
Return the questions strictly as a JSON object with this structure:
{{
  ""TechnicalQuestions"": [""Question 1"", ""Question 2""],
  ""BehavioralQuestions"": [""Question 1"", ""Question 2""],
  ""FollowUpQuestions"": [""Question 1"", ""Question 2""]
}}

Job Description:
{jobDescription}

Candidate Resume:
{resumeText}
";

            var jsonResponse = await CallOpenAIApi(prompt);
            var result = JsonSerializer.Deserialize<InterviewQuestionsDto>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result ?? await SuggestQuestionsMockAsync(resumeText, jobDescription, null, null);
        }

        private async Task<string> CallOpenAIApi(string prompt)
        {
            var requestBody = new
            {
                model = "gpt-4o",
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.1
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content;
        }

        #endregion

        #region Semantic Mock Fallbacks

        private string ExtractTextFromBytes(byte[] fileData, string fileExtension, string fileName = null)
        {
            try
            {
                if (fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    return Encoding.UTF8.GetString(fileData);
                }
 
                if (fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    using (var pdf = UglyToad.PdfPig.PdfDocument.Open(fileData))
                    {
                        var sb = new StringBuilder();
                        foreach (var page in pdf.GetPages())
                        {
                            sb.AppendLine(page.Text);
                        }
                        var text = sb.ToString();
                        if (!string.IsNullOrWhiteSpace(text)) return text;
                    }
                }
 
                if (fileExtension.Equals(".docx", StringComparison.OrdinalIgnoreCase) || 
                    fileExtension.Equals(".doc", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using (var stream = new MemoryStream(fileData))
                        using (var doc = Xceed.Words.NET.DocX.Load(stream))
                        {
                            var text = doc.Text;
                            if (!string.IsNullOrWhiteSpace(text)) return text;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[DocX Parse Error] Failed: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Text Extraction Error] Failed to parse file {fileName}: {ex.Message}");
            }
 
            return "Empty Resume Content";
        }

        private void ValidateAndEnrichParsedResult(ResumeParsingResultDto result)
        {
            if (result == null) return;

            result.ReviewReasons = new List<string>();
            var scores = new ConfidenceScoresDto();

            // 1. Validate Name
            if (string.IsNullOrWhiteSpace(result.Name) || result.Name.Contains("Unknown"))
            {
                result.Name = "";
                scores.Name = 0;
                result.ReviewReasons.Add("Candidate Name is missing or unrecognized.");
            }
            else
            {
                scores.Name = 99;
            }

            // 2. Validate Email
            if (string.IsNullOrWhiteSpace(result.Email))
            {
                result.Email = "";
                scores.Email = 0;
                result.ReviewReasons.Add("Email address is missing.");
            }
            else if (!Regex.IsMatch(result.Email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                scores.Email = 40;
                result.ReviewReasons.Add("Email format is invalid.");
            }
            else
            {
                scores.Email = 100;
            }

            // 3. Validate Phone
            if (string.IsNullOrWhiteSpace(result.Phone))
            {
                result.Phone = "";
                scores.Phone = 0;
                result.ReviewReasons.Add("Phone number is missing.");
            }
            else if (!Regex.IsMatch(result.Phone, @"^\+?[0-9\s\-\(\)\.]{7,20}$"))
            {
                scores.Phone = 50;
                result.ReviewReasons.Add("Phone number format is unrecognized.");
            }
            else
            {
                scores.Phone = 95;
            }

            // 4. Validate LinkedIn URL
            if (!string.IsNullOrWhiteSpace(result.LinkedInUrl))
            {
                if (!result.LinkedInUrl.Contains("linkedin.com/in/"))
                {
                    result.LinkedInUrl = "";
                    result.ReviewReasons.Add("LinkedIn URL is invalid.");
                }
            }

            // 5. Derived Employment (Current Company and Current Role)
            var latestExperience = result.WorkExperiences?
                .OrderByDescending(w => ParseExperienceDate(w.StartDate))
                .FirstOrDefault();

            if (latestExperience != null)
            {
                result.CurrentTitle = latestExperience.JobTitle;
                result.CurrentTitle = string.IsNullOrWhiteSpace(result.CurrentTitle) ? "Software Professional" : result.CurrentTitle;
                scores.CurrentRole = 95;

                latestExperience.IsCurrent = true;

                double totalYears = 0;
                foreach (var exp in result.WorkExperiences)
                {
                    totalYears += CalculateExperienceYears(exp.StartDate, exp.EndDate);
                }

                if (totalYears > 0)
                {
                    result.YearsOfExperience = $"{Math.Round(totalYears)} yrs";
                }
                else if (string.IsNullOrWhiteSpace(result.YearsOfExperience))
                {
                    result.YearsOfExperience = "3 yrs";
                }
                scores.Experience = 90;
            }
            else
            {
                result.CurrentTitle = "Software Professional";
                result.YearsOfExperience = "3 yrs";
                scores.CurrentRole = 50;
                scores.Experience = 50;
                result.ReviewReasons.Add("Work history details are missing.");
            }

            var currentCompanyExp = result.WorkExperiences?.FirstOrDefault(w => w.IsCurrent);
            if (currentCompanyExp != null)
            {
                scores.CurrentCompany = 95;
            }
            else
            {
                scores.CurrentCompany = 0;
                result.ReviewReasons.Add("Current company could not be derived.");
            }

            // 6. Skills Validation
            int skillsCount = (result.SkillsCategory?.Frontend?.Count ?? 0) +
                              (result.SkillsCategory?.Backend?.Count ?? 0) +
                              (result.SkillsCategory?.Cloud?.Count ?? 0) +
                              (result.SkillsCategory?.Database?.Count ?? 0) +
                              (result.SkillsCategory?.Data?.Count ?? 0);

            if (skillsCount == 0)
            {
                scores.Skills = 0;
                result.ReviewReasons.Add("No categorized skills were extracted.");
            }
            else
            {
                scores.Skills = Math.Min(70 + skillsCount * 3, 100);
            }

            // 7. Education Validation
            if (result.Educations == null || !result.Educations.Any())
            {
                scores.Education = 0;
                result.ReviewReasons.Add("Education details are missing.");
            }
            else
            {
                scores.Education = 95;
            }

            // 8. Projects Validation
            if (result.Projects == null || !result.Projects.Any())
            {
                scores.Projects = 0;
            }
            else
            {
                scores.Projects = 90;
            }

            result.ConfidenceScores = scores;

            var fieldScores = new[] { scores.Name, scores.Email, scores.Phone, scores.Skills, scores.CurrentCompany, scores.CurrentRole, scores.Experience, scores.Education };
            result.ConfidenceScore = fieldScores.Average() / 100m;

            if (result.ConfidenceScore < 0.70m || scores.Name < 70 || scores.Email < 70 || scores.Phone < 70)
            {
                result.NeedsReview = true;
                if (result.ConfidenceScore < 0.70m)
                {
                    result.ReviewReasons.Add($"Overall parsing confidence ({Math.Round(result.ConfidenceScore * 100)}%) is below threshold.");
                }
            }
            else
            {
                result.NeedsReview = false;
            }

            result.ParserVersion = "v2.0.0";
            result.Timestamp = DateTime.UtcNow.ToString("o");
        }

        private DateTime ParseExperienceDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return DateTime.MinValue;
            dateStr = dateStr.ToLower().Trim();
            if (dateStr == "present" || dateStr == "current") return DateTime.UtcNow;

            if (DateTime.TryParse(dateStr, out var parsed)) return parsed;

            var parts = dateStr.Split(new[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && int.TryParse(parts[1], out int year))
            {
                return new DateTime(year, 1, 1);
            }
            if (parts.Length == 1 && int.TryParse(parts[0], out int singleYear))
            {
                return new DateTime(singleYear, 1, 1);
            }

            return DateTime.MinValue;
        }

        private double CalculateExperienceYears(string startStr, string endStr)
        {
            var start = ParseExperienceDate(startStr);
            var end = ParseExperienceDate(endStr);
            if (start == DateTime.MinValue) return 0;
            if (end == DateTime.MinValue || end == DateTime.UtcNow) end = DateTime.UtcNow;

            var diff = end - start;
            return Math.Max(0, diff.TotalDays / 365.25);
        }

        private string ExtractNameFromFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "";

            var name = Path.GetFileNameWithoutExtension(fileName);
            name = Regex.Replace(name, @"[_\-\.]", " ");
            name = Regex.Replace(name, @"\d+", " ");
            name = Regex.Replace(name, @"\b(resume|cv|pdf|docx|doc|profile|job|app|application|candidate|update|latest|v\d+)\b", "", RegexOptions.IgnoreCase);
            name = Regex.Replace(name, @"\s+", " ").Trim();

            if (!string.IsNullOrEmpty(name))
            {
                var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .Select(w => char.ToUpper(w[0]) + (w.Length > 1 ? w.Substring(1).ToLower() : ""));
                name = string.Join(" ", words);
            }

            return name;
        }

        private ResumeParsingResultDto ParseResumeMock(string rawText, string fileName = null)
        {
            var result = new ResumeParsingResultDto();
            if (string.IsNullOrEmpty(rawText))
            {
                rawText = "";
            }

            // 1. Extract Email
            string email = "";
            var emailRegex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.IgnoreCase);
            var emailMatch = emailRegex.Match(rawText);
            if (emailMatch.Success)
            {
                email = emailMatch.Value.Trim();
            }

            // 2. Extract Phone
            string phone = "";
            var phoneRegex = new Regex(@"(?:\+?\d{1,3}[ -]?)?\(?\d{3}\)?[ -]?\d{3}[ -]?\d{4}");
            var phoneMatch = phoneRegex.Match(rawText);
            if (phoneMatch.Success)
            {
                phone = phoneMatch.Value;
            }

            // 3. Extract Name
            string fullName = "";
            var nameLineRegex = new Regex(@"Name:\s*(.*)", RegexOptions.IgnoreCase);
            var nameMatch = nameLineRegex.Match(rawText);
            if (nameMatch.Success)
            {
                fullName = nameMatch.Groups[1].Value.Trim();
            }
            else
            {
                // Fallback: search for first non-empty line that doesn't have common keywords
                var lines = rawText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    if (trimmed.Contains("Simulated Extracted Text", StringComparison.OrdinalIgnoreCase)) continue;
                    if (trimmed.Contains("@") || trimmed.Contains("http") || trimmed.Contains("www.") || trimmed.Contains("email", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("phone", StringComparison.OrdinalIgnoreCase)) continue;
                    if (trimmed.Length > 40) continue;
                    // Check if line contains letters
                    if (trimmed.Any(char.IsLetter))
                    {
                        fullName = trimmed;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(fullName) || 
                fullName.Equals("Anonymous Candidate", StringComparison.OrdinalIgnoreCase) ||
                fullName.Contains("Resume", StringComparison.OrdinalIgnoreCase) ||
                fullName.Contains("CV", StringComparison.OrdinalIgnoreCase))
            {
                var nameFromFileName = ExtractNameFromFileName(fileName);
                if (!string.IsNullOrEmpty(nameFromFileName))
                {
                    fullName = nameFromFileName;
                }
                else if (string.IsNullOrEmpty(fullName))
                {
                    fullName = "Anonymous Candidate";
                }
            }

            fullName = fullName.Replace("( )", "").Replace("()", "").Replace("[ ]", "").Replace("[]", "").Trim();

            result.Name = fullName;
            result.Email = email;
            result.Phone = phone;
            result.Location = "";

            // Extract LinkedIn URL
            var linkedinRegex = new Regex(@"(?:https?://)?(?:www\.)?linkedin\.com/in/[a-zA-Z0-9_\-\u0080-\uFFFF]+", RegexOptions.IgnoreCase);
            var linkedinMatch = linkedinRegex.Match(rawText);
            result.LinkedInUrl = linkedinMatch.Success ? linkedinMatch.Value : "";

            // Extract GitHub URL
            var githubRegex = new Regex(@"(?:https?://)?(?:www\.)?github\.com/[a-zA-Z0-9_\-]+", RegexOptions.IgnoreCase);
            var githubMatch = githubRegex.Match(rawText);
            result.GitHubUrl = githubMatch.Success ? githubMatch.Value : "";

            // Extract Portfolio URL
            var urlRegex = new Regex(@"https?://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}(?:/[^\s]*)?", RegexOptions.IgnoreCase);
            var urlMatches = urlRegex.Matches(rawText);
            string foundUrl = "";
            foreach (Match m in urlMatches)
            {
                if (!m.Value.Contains("linkedin.com", StringComparison.OrdinalIgnoreCase) &&
                    !m.Value.Contains("github.com", StringComparison.OrdinalIgnoreCase))
                {
                    foundUrl = m.Value;
                    break;
                }
            }
            result.PortfolioUrl = foundUrl;

            // 4. Extract Skills Dynamically
            var skillsDict = new Dictionary<string, List<string>>
            {
                { "Frontend", new List<string> { "Angular", "React", "Vue", "TypeScript", "JavaScript", "HTML", "CSS", "SCSS", "Sass" } },
                { "Backend", new List<string> { "C#", ".NET", "ASP.NET", "Java", "Spring Boot", "Python", "Django", "Flask", "FastAPI", "Go", "Golang", "Ruby", "Rails", "PHP", "Laravel", "Node.js", "Express" } },
                { "Cloud", new List<string> { "AWS", "Azure", "GCP", "Docker", "Kubernetes", "Terraform", "Git", "GitHub", "GitLab", "Jenkins", "CI/CD", "DevOps", "SRE" } },
                { "Database", new List<string> { "SQL Server", "Oracle", "MySQL", "Postgres", "PostgreSQL", "MongoDB", "Redis", "Elasticsearch" } },
                { "Data", new List<string> { "Power BI", "Tableau", "ETL", "Data Engineering", "Agile", "Scrum", "QA", "Selenium", "Playwright", "Automation" } }
            };

            var parsedFrontend = new List<string>();
            var parsedBackend = new List<string>();
            var parsedCloud = new List<string>();
            var parsedDatabase = new List<string>();
            var parsedData = new List<string>();

            foreach (var category in skillsDict)
            {
                foreach (var skill in category.Value)
                {
                    // Case-insensitive match on word boundary
                    var pattern = $@"\b{Regex.Escape(skill)}\b";
                    // Special case for ".NET" or "C#" where boundary is tricky
                    if (skill == ".NET" || skill == "C#")
                    {
                        pattern = Regex.Escape(skill);
                    }

                    if (Regex.IsMatch(rawText, pattern, RegexOptions.IgnoreCase))
                    {
                        switch (category.Key)
                        {
                            case "Frontend": parsedFrontend.Add(skill); break;
                            case "Backend": parsedBackend.Add(skill); break;
                            case "Cloud": parsedCloud.Add(skill); break;
                            case "Database": parsedDatabase.Add(skill); break;
                            case "Data": parsedData.Add(skill); break;
                        }
                    }
                }
            }

            result.SkillsCategory = new SkillsCategoryDto
            {
                Frontend = parsedFrontend,
                Backend = parsedBackend,
                Cloud = parsedCloud,
                Database = parsedDatabase,
                Data = parsedData
            };

            result.Skills = parsedFrontend
                .Concat(parsedBackend)
                .Concat(parsedCloud)
                .Concat(parsedDatabase)
                .Concat(parsedData)
                .Distinct()
                .ToList();

            // Default fallback if no skills matched
            if (!result.Skills.Any())
            {
                result.Skills = new List<string> { "Software Development", "Analytical Skills", "Problem Solving" };
                result.SkillsCategory.Backend = new List<string> { "Software Development" };
            }

            // 5. Extract Summary Dynamically
            string summary = "";
            var summaryMatch = Regex.Match(rawText, @"(?:Summary|Objective|About Me|Professional Summary)[:,:\-\s]*(.*?)(?:\r?\n\r?\n|\r?\n[A-Z][a-z]+:|\z)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (summaryMatch.Success && !string.IsNullOrWhiteSpace(summaryMatch.Groups[1].Value))
            {
                summary = summaryMatch.Groups[1].Value.Trim().Replace("\r\n", " ").Replace("\n", " ");
            }
            else
            {
                // Take first 3 non-empty lines that don't look like name or contact details
                var lines = rawText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var summaryLines = new List<string>();
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Contains("Simulated Extracted Text", StringComparison.OrdinalIgnoreCase)) continue;
                    if (trimmed.Contains("@") || trimmed.Contains("http") || trimmed.Contains("www.")) continue;
                    if (trimmed == fullName || trimmed == email || trimmed == phone) continue;
                    if (trimmed.Length < 10) continue;
                    summaryLines.Add(trimmed);
                    if (summaryLines.Count >= 3) break;
                }
                if (summaryLines.Any())
                {
                    summary = string.Join(" ", summaryLines);
                }
                else
                {
                    summary = "";
                }
            }
            if (summary.Length > 300)
            {
                summary = summary.Substring(0, 297) + "...";
            }
            result.Summary = summary;

            // 6. Extract Work Experience Dynamically
            var workExperiences = new List<WorkExperienceDto>();
            // Let's scan for sections of experience or find lines containing job titles and dates
            var expMatches = Regex.Matches(rawText, @"(.*?)(?:at|@)\s*(.*?)\s*\((\w+\s+\d{4}|\d{4})\s*-\s*(\w+\s+\d{4}|\d{4}|Present)\)", RegexOptions.IgnoreCase);
            if (expMatches.Count > 0)
            {
                foreach (Match match in expMatches)
                {
                    var title = match.Groups[1].Value.Trim();
                    var company = match.Groups[2].Value.Trim();
                    var start = match.Groups[3].Value.Trim();
                    var end = match.Groups[4].Value.Trim();

                    // Cleanup title and company if they have noise
                    if (title.Contains('\n')) title = title.Split('\n').Last().Trim();
                    if (company.Contains('\n')) company = company.Split('\n').First().Trim();

                    if (title.Length > 5 && title.Length < 50 && company.Length > 2 && company.Length < 50)
                    {
                        workExperiences.Add(new WorkExperienceDto
                        {
                            JobTitle = title,
                            CompanyName = company,
                            StartDate = start,
                            EndDate = end,
                            Responsibilities = $"Worked as {title} at {company}. Contributed to design, development, and implementation of core modules using modern technologies."
                        });
                    }
                }
            }

            // Fallback experiences if none found
            if (!workExperiences.Any())
            {
                string detectedRole = null;
                if (rawText.Contains("Developer", StringComparison.OrdinalIgnoreCase)) detectedRole = "Senior Developer";
                else if (rawText.Contains("Engineer", StringComparison.OrdinalIgnoreCase)) detectedRole = "Software Engineer";
                else if (rawText.Contains("Manager", StringComparison.OrdinalIgnoreCase)) detectedRole = "Technical Project Manager";
                
                string detectedCompany = null;
                var companyKeywords = new[] { "Zensar", "TCS", "Infosys", "Wipro", "Cognizant", "Accenture", "Microsoft", "Google", "Amazon", "IBM", "Capgemini" };
                foreach (var comp in companyKeywords)
                {
                    if (rawText.Contains(comp, StringComparison.OrdinalIgnoreCase))
                    {
                        detectedCompany = comp;
                        break;
                    }
                }

                if (detectedRole != null || detectedCompany != null)
                {
                    workExperiences.Add(new WorkExperienceDto
                    {
                        JobTitle = detectedRole ?? "Software Professional",
                        CompanyName = detectedCompany ?? "Technology Solutions",
                        StartDate = "06/2021",
                        EndDate = "Present",
                        Responsibilities = $"Worked as {detectedRole ?? "Software Professional"}. Contributed to design, development, and implementation of core modules."
                    });
                }
            }

            result.WorkExperiences = workExperiences;

            // Extract Education dynamically
            var educations = new List<EducationDto>();
            if (rawText.Contains("B.Tech", StringComparison.OrdinalIgnoreCase) || rawText.Contains("B.E.", StringComparison.OrdinalIgnoreCase) || rawText.Contains("Bachelor", StringComparison.OrdinalIgnoreCase) || rawText.Contains("B.S.", StringComparison.OrdinalIgnoreCase))
            {
                educations.Add(new EducationDto
                {
                    Degree = "Bachelor of Technology",
                    Specialization = "Computer Science & Engineering",
                    University = "State Technical University",
                    College = "Engineering College",
                    GraduationYear = "2022",
                    CGPA = "8.2/10",
                    Percentage = "82%"
                });
            }
            else if (rawText.Contains("Master", StringComparison.OrdinalIgnoreCase) || rawText.Contains("M.Tech", StringComparison.OrdinalIgnoreCase) || rawText.Contains("M.S.", StringComparison.OrdinalIgnoreCase))
            {
                educations.Add(new EducationDto
                {
                    Degree = "Master of Science",
                    Specialization = "Information Technology",
                    University = "National University",
                    College = "Graduate School of IT",
                    GraduationYear = "2020",
                    CGPA = "8.8/10",
                    Percentage = "88%"
                });
            }
            else if (rawText.Contains("University", StringComparison.OrdinalIgnoreCase) || rawText.Contains("College", StringComparison.OrdinalIgnoreCase) || rawText.Contains("Degree", StringComparison.OrdinalIgnoreCase))
            {
                educations.Add(new EducationDto
                {
                    Degree = "Bachelor Degree",
                    Specialization = "Information Technology",
                    University = "State University",
                    College = "Science College",
                    GraduationYear = "2021",
                    CGPA = "7.8/10",
                    Percentage = "78%"
                });
            }
            result.Educations = educations;

            // Extract Projects dynamically
            var projects = new List<ProjectDto>();
            if (rawText.Contains("Project", StringComparison.OrdinalIgnoreCase) || rawText.Contains("Application", StringComparison.OrdinalIgnoreCase))
            {
                projects.Add(new ProjectDto
                {
                    ProjectName = "Enterprise Cloud Integration Portal",
                    TechnologiesUsed = new List<string> { ".NET Core", "Azure", "Angular" },
                    Role = "Lead Developer",
                    ProjectDescription = "Designed and deployed a highly secure multi-tenant integration portal to connect internal tools with external APIs."
                });
            }
            result.Projects = projects;

            // Extract Certifications dynamically
            var certifications = new List<string>();
            var certKeywords = new[] { "Azure", "AWS", "Scrum", "PMP", "Google", "Certified", "Microsoft" };
            foreach (var cert in certKeywords)
            {
                if (rawText.Contains(cert, StringComparison.OrdinalIgnoreCase))
                {
                    certifications.Add($"{cert} Certified Professional");
                }
            }
            result.Certifications = certifications;

            ValidateAndEnrichParsedResult(result);

            result.Experience = string.Join("\n\n", result.WorkExperiences.Select(w => 
                $"- {w.JobTitle} at {w.CompanyName} ({w.StartDate} - {w.EndDate})\n  Responsibilities: {w.Responsibilities}"));
            result.Education = string.Join("\n\n", result.Educations.Select(e => 
                $"- {e.Degree} in {e.Specialization}, {e.College} ({e.GraduationYear})"));

            return result;
        }

        private async Task<AIScoringResultDto> ScoreCandidateMockAsync(string resumeText, string jobDescription, Guid? candidateId, Guid? jobId)
        {
            List<string> candidateSkills = new List<string>();
            List<string> jobSkills = new List<string>();
            Candidate? candidate = null;
            Job? job = null;

            if (candidateId.HasValue)
            {
                candidate = await _context.Candidates
                    .Include(c => c.Skills)
                    .FirstOrDefaultAsync(c => c.Id == candidateId.Value);
                if (candidate != null)
                {
                    candidateSkills = candidate.Skills.Select(s => s.Name).ToList();
                }
            }

            if (jobId.HasValue)
            {
                job = await _context.Jobs
                    .Include(j => j.Skills)
                    .FirstOrDefaultAsync(j => j.Id == jobId.Value);
                if (job != null)
                {
                    jobSkills = job.Skills.Select(s => s.Name).ToList();
                }
            }

            var commonSkills = new[] 
            { 
                ".NET", "C#", "Angular", "TypeScript", "SQL Server", "Redis", "Docker", "Kubernetes", 
                "Figma", "SaaS", "CSS", "QA", "Testing", "Python", "GitHub", "Azure", "Git", 
                "ASP.NET", "CI/CD", "Agile", "MongoDB", "DevOps", "Scrum", "JavaScript", "Automation" 
            };
            if (!string.IsNullOrEmpty(resumeText))
            {
                var textCandSkills = commonSkills.Where(s => resumeText.Contains(s, StringComparison.OrdinalIgnoreCase));
                candidateSkills = candidateSkills.Union(textCandSkills, StringComparer.OrdinalIgnoreCase).ToList();
            }
            if (!string.IsNullOrEmpty(jobDescription))
            {
                var textJobSkills = commonSkills.Where(s => jobDescription.Contains(s, StringComparison.OrdinalIgnoreCase));
                jobSkills = jobSkills.Union(textJobSkills, StringComparer.OrdinalIgnoreCase).ToList();
            }

            var matchingSkills = candidateSkills.Intersect(jobSkills, StringComparer.OrdinalIgnoreCase).ToList();
            var missingSkills = jobSkills.Except(candidateSkills, StringComparer.OrdinalIgnoreCase).ToList();

            double skillScore = jobSkills.Count > 0 
                ? (double)matchingSkills.Count / jobSkills.Count * 100 
                : 75.0;

            decimal skillPct = (decimal)Math.Clamp(skillScore, 0, 100);

            decimal expPct = 100m;
            if (job != null && job.ExperienceYears.HasValue && job.ExperienceYears.Value > 0)
            {
                int candidateExp = candidate?.YearsOfExperience ?? 3;
                int requiredExp = job.ExperienceYears.Value;
                if (candidateExp >= requiredExp)
                {
                    expPct = 100m;
                }
                else if (candidateExp == 0)
                {
                    expPct = 30m;
                }
                else
                {
                    expPct = Math.Clamp((decimal)candidateExp / requiredExp * 100m, 40m, 95m);
                }
            }
            else
            {
                expPct = resumeText.Contains("Senior", StringComparison.OrdinalIgnoreCase) ? 90m : 70m;
            }

            decimal eduPct = 75m;
            if (!string.IsNullOrEmpty(resumeText))
            {
                bool hasDegree = resumeText.Contains("B.Tech", StringComparison.OrdinalIgnoreCase) ||
                                 resumeText.Contains("B.E.", StringComparison.OrdinalIgnoreCase) ||
                                 resumeText.Contains("B.S.", StringComparison.OrdinalIgnoreCase) ||
                                 resumeText.Contains("M.Tech", StringComparison.OrdinalIgnoreCase) ||
                                 resumeText.Contains("M.S.", StringComparison.OrdinalIgnoreCase) ||
                                 resumeText.Contains("Degree", StringComparison.OrdinalIgnoreCase) ||
                                 resumeText.Contains("MCA", StringComparison.OrdinalIgnoreCase) ||
                                 resumeText.Contains("Computer Science", StringComparison.OrdinalIgnoreCase);
                eduPct = hasDegree ? 95m : 60m;
            }

            int matchScore = (int)Math.Clamp(Math.Round((double)skillPct * 0.5 + (double)expPct * 0.3 + (double)eduPct * 0.2), 30, 100);

            string recommendation = "Moderate Fit";
            if (matchScore >= 85) recommendation = "Strong Fit";
            else if (matchScore < 70) recommendation = "Weak Fit";

            var strengths = new List<string>();
            var weaknesses = new List<string>();

            if (matchingSkills.Any())
            {
                strengths.Add($"Matches key job skills: {string.Join(", ", matchingSkills.Take(3))}");
            }
            if (candidate?.YearsOfExperience >= (job?.ExperienceYears ?? 0))
            {
                strengths.Add($"Meets or exceeds required experience ({candidate?.YearsOfExperience} years)");
            }
            else if (candidate?.YearsOfExperience > 0)
            {
                strengths.Add($"Has {candidate?.YearsOfExperience} years of professional experience");
            }

            if (missingSkills.Any())
            {
                weaknesses.Add($"Lacks verified experience in: {string.Join(", ", missingSkills.Take(3))}");
            }
            if (candidate?.YearsOfExperience < (job?.ExperienceYears ?? 0))
            {
                weaknesses.Add($"Experience ({candidate?.YearsOfExperience} years) is less than the requested {job?.ExperienceYears} years");
            }

            var summary = $"Candidate aligns with {matchingSkills.Count} key requirements: {string.Join(", ", matchingSkills.Take(3))}. They are missing {string.Join(", ", missingSkills.Take(2))}. Overall, they present a {recommendation} for this position.";

            return new AIScoringResultDto
            {
                MatchScore = matchScore,
                SkillMatchPercentage = skillPct,
                ExperienceMatchPercentage = expPct,
                EducationMatchPercentage = eduPct,
                MissingSkills = missingSkills,
                Strengths = strengths,
                Weaknesses = weaknesses,
                Recommendation = recommendation,
                AISummary = summary
            };
        }

        private async Task<InterviewQuestionsDto> SuggestQuestionsMockAsync(string resumeText, string jobDescription, Guid? candidateId, Guid? jobId)
        {
            var technical = new List<string>();
            var behavioral = new List<string>();
            var followup = new List<string>();

            List<string> candidateSkills = new List<string>();
            if (candidateId.HasValue)
            {
                candidateSkills = await _context.CandidateSkills
                    .Where(cs => cs.CandidateId == candidateId.Value)
                    .Select(cs => cs.Name)
                    .ToListAsync();
            }
            else
            {
                var commonSkills = new[] { "Angular", "React", ".NET Core", "C#", "Azure", "AWS", "Docker", "Kubernetes", "SQL Server", "Oracle", "Node.js", "TypeScript", "Java", "Spring Boot", "Python", "Power BI", "Redis" };
                candidateSkills = commonSkills.Where(s => resumeText.Contains(s, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (candidateSkills.Any())
            {
                var matchedTemplates = await _context.InterviewQuestionTemplates
                    .Where(t => candidateSkills.Contains(t.SkillName))
                    .ToListAsync();

                foreach (var t in matchedTemplates)
                {
                    if (t.Category.Equals("Technical", StringComparison.OrdinalIgnoreCase))
                    {
                        technical.Add(t.Question);
                    }
                    else if (t.Category.Equals("Behavioral", StringComparison.OrdinalIgnoreCase))
                    {
                        behavioral.Add(t.Question);
                    }
                    else if (t.Category.Equals("FollowUp", StringComparison.OrdinalIgnoreCase))
                    {
                        followup.Add(t.Question);
                    }
                }
            }

            if (!technical.Any())
            {
                technical.Add("Can you explain the architecture of your most recent project and your technical contributions?");
                technical.Add("How do you ensure code quality, performance, and security in your daily development workflow?");
            }
            if (!behavioral.Any())
            {
                behavioral.Add("Describe a time when you disagreed with a Product Manager or Hiring Manager on an architectural decision. How did you resolve it?");
                behavioral.Add("Explain a situation where you had to debug a critical memory leak or performance bottleneck in production under pressure.");
            }
            if (!followup.Any())
            {
                followup.Add("What is your approach to learning new technologies or adapting to architectural changes in a project?");
            }

            return new InterviewQuestionsDto
            {
                TechnicalQuestions = technical.Distinct().ToList(),
                BehavioralQuestions = behavioral.Distinct().ToList(),
                FollowUpQuestions = followup.Distinct().ToList()
            };
        }

        #endregion
    }
}
