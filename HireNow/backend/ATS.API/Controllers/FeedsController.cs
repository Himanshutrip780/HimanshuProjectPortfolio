using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ATS.Application.Features.Jobs;
using ATS.Domain.Enums;
using ATS.Shared.Models;

namespace ATS.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class FeedsController : ApiControllerBase
    {
        [HttpGet("{companyId}/indeed")]
        public async Task<IActionResult> GetIndeedFeed(Guid companyId)
        {
            var result = await Mediator.Send(new GetJobFeedQuery(companyId));
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<source>");
            sb.AppendLine("  <publisher>HireNow ATS</publisher>");
            sb.AppendLine("  <publisherurl>http://localhost:4200</publisherurl>");
            sb.AppendLine($"  <lastBuildDate>{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</lastBuildDate>");

            foreach (var job in result.Value)
            {
                sb.AppendLine("  <job>");
                sb.AppendLine($"    <title><![CDATA[{job.Title}]]></title>");
                sb.AppendLine($"    <date><![CDATA[{job.CreatedDate:r}]]></date>");
                sb.AppendLine($"    <referencenumber><![CDATA[{job.Id}]]></referencenumber>");
                sb.AppendLine($"    <url><![CDATA[http://localhost:4200/careers?companyId={companyId}&jobId={job.Id}]]></url>");
                sb.AppendLine($"    <company><![CDATA[{job.CompanyName}]]></company>");
                sb.AppendLine("    <sourcename><![CDATA[HireNow ATS]]></sourcename>");

                // Build a rich description combining Description, Responsibilities, and Qualifications
                var descriptionBuilder = new StringBuilder();
                descriptionBuilder.AppendLine(job.Description);
                if (!string.IsNullOrWhiteSpace(job.Responsibilities))
                {
                    descriptionBuilder.AppendLine("<h3>Responsibilities</h3><ul>");
                    foreach (var resp in job.Responsibilities.Split(';'))
                    {
                        if (!string.IsNullOrWhiteSpace(resp))
                        {
                            descriptionBuilder.AppendLine($"<li>{resp.Trim()}</li>");
                        }
                    }
                    descriptionBuilder.AppendLine("</ul>");
                }
                if (!string.IsNullOrWhiteSpace(job.Qualifications))
                {
                    descriptionBuilder.AppendLine("<h3>Qualifications</h3><ul>");
                    foreach (var qual in job.Qualifications.Split(';'))
                    {
                        if (!string.IsNullOrWhiteSpace(qual))
                        {
                            descriptionBuilder.AppendLine($"<li>{qual.Trim()}</li>");
                        }
                    }
                    descriptionBuilder.AppendLine("</ul>");
                }

                sb.AppendLine($"    <description><![CDATA[{descriptionBuilder}]]></description>");
                sb.AppendLine($"    <location><![CDATA[{job.Location}]]></location>");
                sb.AppendLine($"    <category><![CDATA[{job.DepartmentName}]]></category>");
                sb.AppendLine($"    <jobtype><![CDATA[{MapEmploymentTypeToIndeed(job.EmploymentType)}]]></jobtype>");
                sb.AppendLine("  </job>");
            }

            sb.AppendLine("</source>");

            return Content(sb.ToString(), "application/xml", Encoding.UTF8);
        }

        [HttpGet("{companyId}/google")]
        public async Task<IActionResult> GetGoogleJobsFeed(Guid companyId)
        {
            var result = await Mediator.Send(new GetJobFeedQuery(companyId));
            if (!result.IsSuccess)
            {
                return NotFound(result.Error);
            }

            var googleJobs = new List<object>();

            foreach (var job in result.Value)
            {
                var descriptionBuilder = new StringBuilder();
                descriptionBuilder.AppendLine(job.Description);
                if (!string.IsNullOrWhiteSpace(job.Responsibilities))
                {
                    descriptionBuilder.AppendLine("<h3>Responsibilities</h3><ul>");
                    foreach (var resp in job.Responsibilities.Split(';'))
                    {
                        if (!string.IsNullOrWhiteSpace(resp))
                        {
                            descriptionBuilder.AppendLine($"<li>{resp.Trim()}</li>");
                        }
                    }
                    descriptionBuilder.AppendLine("</ul>");
                }
                if (!string.IsNullOrWhiteSpace(job.Qualifications))
                {
                    descriptionBuilder.AppendLine("<h3>Qualifications</h3><ul>");
                    foreach (var qual in job.Qualifications.Split(';'))
                    {
                        if (!string.IsNullOrWhiteSpace(qual))
                        {
                            descriptionBuilder.AppendLine($"<li>{qual.Trim()}</li>");
                        }
                    }
                    descriptionBuilder.AppendLine("</ul>");
                }

                var jobPosting = new Dictionary<string, object>
                {
                    { "@context", "https://schema.org" },
                    { "@type", "JobPosting" },
                    { "title", job.Title },
                    { "description", descriptionBuilder.ToString() },
                    { "datePosted", job.CreatedDate.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                    { "employmentType", MapEmploymentTypeToGoogle(job.EmploymentType) },
                    { "hiringOrganization", new Dictionary<string, string>
                        {
                            { "@type", "Organization" },
                            { "name", job.CompanyName },
                            { "sameAs", $"http://localhost:4200/careers?companyId={companyId}" },
                            { "logo", job.CompanyLogoUrl }
                        }
                    },
                    { "jobLocation", new Dictionary<string, object>
                        {
                            { "@type", "Place" },
                            { "address", new Dictionary<string, string>
                                {
                                    { "@type", "PostalAddress" },
                                    { "addressLocality", job.Location },
                                    { "addressCountry", "US" }
                                }
                            }
                        }
                    }
                };

                if (job.SalaryMin.HasValue || job.SalaryMax.HasValue)
                {
                    var baseSalary = new Dictionary<string, object>
                    {
                        { "@type", "MonetaryAmount" },
                        { "currency", "USD" }
                    };

                    var value = new Dictionary<string, object>
                    {
                        { "@type", "QuantitativeValue" },
                        { "unitText", "YEAR" }
                    };

                    if (job.SalaryMin.HasValue) value.Add("minValue", job.SalaryMin.Value);
                    if (job.SalaryMax.HasValue) value.Add("maxValue", job.SalaryMax.Value);

                    baseSalary.Add("value", value);
                    jobPosting.Add("baseSalary", baseSalary);
                }

                googleJobs.Add(jobPosting);
            }

            return Ok(googleJobs);
        }

        private static string MapEmploymentTypeToIndeed(EmploymentType type)
        {
            return type switch
            {
                EmploymentType.FullTime => "full-time",
                EmploymentType.PartTime => "part-time",
                EmploymentType.Contract => "contract",
                EmploymentType.Internship => "internship",
                EmploymentType.Temporary => "temporary",
                _ => "full-time"
            };
        }

        private static string MapEmploymentTypeToGoogle(EmploymentType type)
        {
            return type switch
            {
                EmploymentType.FullTime => "FULL_TIME",
                EmploymentType.PartTime => "PART_TIME",
                EmploymentType.Contract => "CONTRACTOR",
                EmploymentType.Internship => "INTERN",
                EmploymentType.Temporary => "TEMPORARY",
                _ => "OTHER"
            };
        }
    }
}
