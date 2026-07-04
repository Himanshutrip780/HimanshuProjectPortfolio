using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Jobs;
using ATS.Application.Features.Candidates;
using ATS.Domain.Enums;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,Recruiter,HiringManager")]
    public class JobsController : ApiControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<Result<PaginatedList<JobDto>>>> GetJobs(
            [FromQuery] Guid? departmentId,
            [FromQuery] JobStatus? status,
            [FromQuery] string? searchTerm,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await Mediator.Send(new GetJobsQuery
            {
                CompanyId = CompanyId,
                DepartmentId = departmentId,
                Status = status,
                SearchTerm = searchTerm,
                PageIndex = pageIndex,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Result<JobDto>>> GetJob(Guid id)
        {
            var result = await Mediator.Send(new GetJobByIdQuery(id, CompanyId));
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result<Guid>>> Create(CreateJobCommand command)
        {
            // Bind tenant company context from token
            var commandWithCompany = command with { CompanyId = CompanyId };
            var result = await Mediator.Send(commandWithCompany);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result>> Update(Guid id, UpdateJobCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(Result.Failure("Mismatched job identifier."));
            }

            var result = await Mediator.Send(command);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result>> Delete(Guid id)
        {
            var result = await Mediator.Send(new DeleteJobCommand(id));
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "SuperAdmin,Recruiter")]
        public async Task<ActionResult<Result>> UpdateStatus(Guid id, [FromBody] UpdateStatusModel model)
        {
            var result = await Mediator.Send(new UpdateJobStatusCommand(id, model.Status));
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpGet("departments")]
        public async Task<ActionResult<Result<List<DepartmentDto>>>> GetDepartments()
        {
            var result = await Mediator.Send(new GetDepartmentsQuery(CompanyId));
            return Ok(result);
        }

        [HttpGet("users")]
        public async Task<ActionResult<Result<List<UserDto>>>> GetUsers([FromQuery] string? role)
        {
            var result = await Mediator.Send(new GetUsersQuery { CompanyId = CompanyId, Role = role });
            return Ok(result);
        }

        [HttpGet("currencies")]
        public ActionResult<Result<List<CurrencyDto>>> GetCurrencies()
        {
            var list = new List<CurrencyDto>
            {
                new() { Value = "USD", Label = "USD ($)" },
                new() { Value = "EUR", Label = "EUR (€)" },
                new() { Value = "GBP", Label = "GBP (£)" },
                new() { Value = "INR", Label = "INR (₹)" },
                new() { Value = "CAD", Label = "CAD ($)" },
                new() { Value = "AUD", Label = "AUD ($)" },
                new() { Value = "SGD", Label = "SGD ($)" }
            };
            return Ok(Result<List<CurrencyDto>>.Success(list));
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<ActionResult<Result<PaginatedList<JobDto>>>> GetPublicJobs(
            [FromQuery] Guid? companyId,
            [FromQuery] Guid? departmentId,
            [FromQuery] string? searchTerm,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await Mediator.Send(new GetJobsQuery
            {
                CompanyId = companyId ?? Guid.Empty,
                DepartmentId = departmentId,
                Status = JobStatus.Published,
                SearchTerm = searchTerm,
                PageIndex = pageIndex,
                PageSize = pageSize
            });

            return Ok(result);
        }

        [HttpGet("public/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Result<JobDto>>> GetPublicJob(Guid id)
        {
            var result = await Mediator.Send(new GetJobByIdQuery(id));
            if (!result.IsSuccess)
            {
                return NotFound(result);
            }
            if (result.Value.Status != JobStatus.Published)
            {
                return BadRequest(Result<JobDto>.Failure("Job is not published."));
            }
            return Ok(result);
        }

        [HttpPost("public/{id}/apply")]
        [AllowAnonymous]
        public async Task<ActionResult<Result<Guid>>> PublicApply(Guid id, [FromForm] PublicApplyModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                return BadRequest(Result<Guid>.Failure("Resume file is required."));
            }

            var sanitizedFileName = Path.GetFileName(model.File.FileName);
            var extension = Path.GetExtension(sanitizedFileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".docx", ".doc", ".txt", ".rtf" };
            if (!allowedExtensions.Contains(extension))
            {
                throw new ATS.Application.Common.Exceptions.ValidationException(new[] { 
                    new FluentValidation.Results.ValidationFailure("File", "Invalid file extension. Only .pdf, .docx, .doc, .txt, and .rtf are allowed.") 
                });
            }

            var headerBytes = new byte[4];
            using (var stream = model.File.OpenReadStream())
            {
                var bytesRead = await stream.ReadAsync(headerBytes, 0, 4);
                if (!ValidateMagicBytes(extension, headerBytes, bytesRead))
                {
                    throw new ATS.Application.Common.Exceptions.ValidationException(new[] { 
                        new FluentValidation.Results.ValidationFailure("File", "Invalid file content. The file header does not match the allowed formats.") 
                    });
                }
            }

            var jobResult = await Mediator.Send(new GetJobByIdQuery(id));
            if (!jobResult.IsSuccess || jobResult.Value == null)
            {
                return BadRequest(Result<Guid>.Failure("Job requisition not found."));
            }

            using var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);

            var command = new UploadResumeCommand
            {
                FileName = sanitizedFileName,
                FileData = memoryStream.ToArray(),
                CompanyId = jobResult.Value.CompanyId,
                JobId = id,
                CandidateFirstName = model.FirstName,
                CandidateLastName = model.LastName,
                CandidateEmail = model.Email,
                CandidatePhone = model.Phone,
                Source = "Careers Portal"
            };

            var result = await Mediator.Send(command);
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

    public class UpdateStatusModel
    {
        public JobStatus Status { get; set; }
    }

    public class PublicApplyModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string? LinkedInUrl { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile File { get; set; }
    }

    public class CurrencyDto
    {
        public string Value { get; set; }
        public string Label { get; set; }
    }
}
