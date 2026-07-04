using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ATS.Application.Features.Candidates;
using ATS.Shared.Models;
using ATS.Application.Common.Interfaces;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter,HiringManager")]
    public class CandidatesController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Result<PaginatedList<CandidateDto>>>> GetCandidates(
            [FromQuery] string? searchTerm,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await Mediator.Send(new GetCandidatesQuery
            {
                CompanyId = CompanyId,
                SearchTerm = searchTerm,
                PageIndex = pageIndex,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<CandidateDto>>> GetCandidate(Guid id)
        {
            var result = await Mediator.Send(new GetCandidateByIdQuery(id, CompanyId));
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result<Guid>>> Create(CreateCandidateCommand command)
        {
            var commandWithCompany = command with { CompanyId = CompanyId };
            var result = await Mediator.Send(commandWithCompany);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("upload-resume")]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result<Guid>>> UploadResume(
            [FromForm] IFormFile file, 
            [FromForm] Guid? jobId)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(Result<Guid>.Failure("No resume file uploaded or file is empty."));
            }

            var sanitizedFileName = Path.GetFileName(file.FileName);
            var extension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".txt", ".rtf" };
            if (!allowedExtensions.Contains(extension))
            {
                throw new ATS.Application.Common.Exceptions.ValidationException(new[] { 
                    new FluentValidation.Results.ValidationFailure("File", "Invalid file extension. Only .pdf, .docx, .doc, .txt, and .rtf are allowed.") 
                });
            }

            var headerBytes = new byte[4];
            using (var stream = file.OpenReadStream())
            {
                var bytesRead = await stream.ReadAsync(headerBytes, 0, 4);
                if (!ValidateMagicBytes(extension, headerBytes, bytesRead))
                {
                    throw new ATS.Application.Common.Exceptions.ValidationException(new[] { 
                        new FluentValidation.Results.ValidationFailure("File", "Invalid file content. The file header does not match the allowed formats.") 
                    });
                }
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var command = new UploadResumeCommand
            {
                FileName = sanitizedFileName,
                FileData = memoryStream.ToArray(),
                CompanyId = CompanyId,
                JobId = jobId
            };

            var result = await Mediator.Send(command);
            if (!result.IsSuccess)
            {
                return Ok(result);
            }

            return Ok(result);
        }

        [HttpGet("{id}/resume")]
        public async Task<IActionResult> DownloadResume(Guid id)
        {
            var candidateResult = await Mediator.Send(new GetCandidateByIdQuery(id, CompanyId));
            if (!candidateResult.IsSuccess || candidateResult.Value == null)
            {
                return NotFound(Result<object>.Failure("Candidate not found."));
            }

            var candidate = candidateResult.Value;
            if (string.IsNullOrEmpty(candidate.ResumePath))
            {
                return NotFound(Result<object>.Failure("Resume file not found for this candidate."));
            }

            try
            {
                var storageService = HttpContext.RequestServices.GetRequiredService<IStorageService>();
                var fileBytes = await storageService.DownloadFileAsync(candidate.ResumePath);
                
                var contentType = "application/pdf";
                if (candidate.ResumePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }
                else if (candidate.ResumePath.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "application/msword";
                }
                else if (candidate.ResumePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    contentType = "text/plain";
                }

                var fileName = Path.GetFileName(candidate.ResumePath);
                var underscoreIndex = fileName.IndexOf('_');
                if (underscoreIndex != -1)
                {
                    fileName = fileName.Substring(underscoreIndex + 1);
                }

                return File(fileBytes, contentType, fileName);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(Result<object>.Failure(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, Result<object>.Failure($"An error occurred while downloading the resume: {ex.Message}"));
            }
        }

        [HttpGet("{id}/duplicates")]
        public async Task<ActionResult<Result<System.Collections.Generic.List<CandidateDto>>>> GetDuplicates(Guid id)
        {
            var result = await Mediator.Send(new GetCandidateDuplicatesQuery(id, CompanyId));
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("{id}/talent-pool")]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result>> AssignTalentPool(Guid id, [FromBody] TalentPoolModel model)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? $"{User.FindFirstValue("FirstName")} {User.FindFirstValue("LastName")}";
            var result = await Mediator.Send(new AssignTalentPoolCommand
            {
                CandidateId = id,
                CompanyId = CompanyId,
                PoolName = model.PoolName,
                Actor = userName
            });

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private static bool ValidateMagicBytes(string extension, byte[] headerBytes, int bytesRead)
        {
            if (bytesRead < 2) return false;

            // Block PE executables
            if (headerBytes[0] == 0x4D && headerBytes[1] == 0x5A)
            {
                return false;
            }

            switch (extension)
            {
                case ".pdf":
                    return bytesRead >= 4 && headerBytes[0] == 0x25 && headerBytes[1] == 0x50 && headerBytes[2] == 0x44 && headerBytes[3] == 0x46;
                case ".docx":
                    return bytesRead >= 2 && headerBytes[0] == 0x50 && headerBytes[1] == 0x4B;
                case ".doc":
                    return bytesRead >= 4 && headerBytes[0] == 0xD0 && headerBytes[1] == 0xCF && headerBytes[2] == 0x11 && headerBytes[3] == 0xE0;
                case ".rtf":
                    return bytesRead >= 4 && headerBytes[0] == 0x7B && headerBytes[1] == 0x5C && headerBytes[2] == 0x72 && headerBytes[3] == 0x74;
                case ".txt":
                    // Block ELF executables
                    if (bytesRead >= 4 && headerBytes[0] == 0x7F && headerBytes[1] == 0x45 && headerBytes[2] == 0x4C && headerBytes[3] == 0x46)
                    {
                        return false;
                    }
                    return true;
                default:
                    return false;
            }
        }
    }

    public class TalentPoolModel
    {
        public string PoolName { get; set; }
    }
}
