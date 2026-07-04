using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectApi.Data;
using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workflow.IO.Shared.Contracts;
using AutoMapper;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly ProjectDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenantContext;

        public ClientController(ProjectDbContext dbContext, IMapper mapper, ITenantContext tenantContext)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _tenantContext = tenantContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            var orgId = _tenantContext.CurrentOrganizationId;
            var clients = await _dbContext.Clients
                .Where(c => c.OrganizationId == orgId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var response = clients.Select(c => new ClientResponseDto
            {
                ClientId = c.ClientId,
                Name = c.Name,
                Industry = c.Industry,
                ContactPerson = c.ContactPerson,
                Email = c.Email,
                Keywords = c.Keywords
            });

            return Ok(ApiResponse<IEnumerable<ClientResponseDto>>.Ok(response));
        }

        [HttpGet("{clientId:guid}")]
        public async Task<IActionResult> GetClientById(Guid clientId)
        {
            var orgId = _tenantContext.CurrentOrganizationId;
            var client = await _dbContext.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId && c.OrganizationId == orgId);
            if (client == null)
            {
                return NotFound();
            }

            var response = new ClientResponseDto
            {
                ClientId = client.ClientId,
                Name = client.Name,
                Industry = client.Industry,
                ContactPerson = client.ContactPerson,
                Email = client.Email,
                Keywords = client.Keywords
            };

            return Ok(ApiResponse<ClientResponseDto>.Ok(response));
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Client name is required");
            }
            var orgId = _tenantContext.CurrentOrganizationId;
            if (orgId == null)
            {
                return BadRequest("No active workspace found.");
            }

            var client = new Client(
                request.Name,
                request.Industry,
                request.ContactPerson,
                request.Email,
                request.Keywords,
                orgId.Value
            );

            _dbContext.Clients.Add(client);
            await _dbContext.SaveChangesAsync();

            var response = new ClientResponseDto
            {
                ClientId = client.ClientId,
                Name = client.Name,
                Industry = client.Industry,
                ContactPerson = client.ContactPerson,
                Email = client.Email,
                Keywords = client.Keywords
            };

            return Ok(ApiResponse<ClientResponseDto>.Ok(response, "Client created successfully"));
        }

        [HttpPut("{clientId:guid}")]
        public async Task<IActionResult> UpdateClient(Guid clientId, [FromBody] UpdateClientRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Client name is required");
            }

            var client = await _dbContext.Clients.FindAsync(clientId);
            if (client == null)
            {
                return NotFound();
            }

            client.Update(
                request.Name,
                request.Industry,
                request.ContactPerson,
                request.Email,
                request.Keywords
            );

            await _dbContext.SaveChangesAsync();

            var response = new ClientResponseDto
            {
                ClientId = client.ClientId,
                Name = client.Name,
                Industry = client.Industry,
                ContactPerson = client.ContactPerson,
                Email = client.Email,
                Keywords = client.Keywords
            };

            return Ok(ApiResponse<ClientResponseDto>.Ok(response, "Client updated successfully"));
        }

        [HttpDelete("{clientId:guid}")]
        public async Task<IActionResult> DeleteClient(Guid clientId)
        {
            var client = await _dbContext.Clients.FindAsync(clientId);
            if (client == null)
            {
                return NotFound();
            }

            _dbContext.Clients.Remove(client);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }
    }

    public class ClientResponseDto
    {
        public Guid ClientId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
    }

    public class CreateClientRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
    }

    public class UpdateClientRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Keywords { get; set; } = string.Empty;
    }
}
