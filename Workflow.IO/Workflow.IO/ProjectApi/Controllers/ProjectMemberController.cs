using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectApi.Model.Domain.Entities;
using ProjectApi.Model.Dto;
using ProjectApi.Repositories;
using System.Security.Claims;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.Exceptions;
using Workflow.IO.Shared.Persistence;

namespace ProjectApi.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/members")]
    [Authorize]
    public class ProjectMemberController : ControllerBase
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectMemberController(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _unitOfWork = unitOfWork;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
                throw new UnauthorizedAccessException();
            return Guid.Parse(userIdClaim);
        }

        private async Task EnsureAccessAsync(Guid projectId)
        {
            var userId = GetCurrentUserId();
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) throw new NotFoundException("Project not found");

            if (project.OwnerId != userId)
            {
                var isMember = await _projectRepository.IsMemberAsync(projectId, userId);
                if (!isMember) throw new UnauthorizedAccessException("You don't have access to this project");
            }
        }

        private async Task EnsureAdminAccessAsync(Guid projectId)
        {
            var userId = GetCurrentUserId();
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) throw new NotFoundException("Project not found");

            if (project.OwnerId != userId)
            {
                var member = await _projectRepository.GetMemberAsync(projectId, userId);
                if (member == null || (member.Role != Model.Domain.Enums.ProjectRole.Owner && member.Role != Model.Domain.Enums.ProjectRole.Admin))
                    throw new UnauthorizedAccessException("You don't have permission to manage members");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMembers(Guid projectId)
        {
            await EnsureAccessAsync(projectId);

            var members = await _projectRepository.GetMembersAsync(projectId);
            var dtos = members.Select(m => new ProjectMemberDto
            {
                ProjectId = m.ProjectId,
                UserId = m.UserId,
                Role = m.Role,
                JoinedAt = m.JoinedAt
            });

            return Ok(ApiResponse<IEnumerable<ProjectMemberDto>>.Ok(dtos));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyMembership(Guid projectId)
        {
            var userId = GetCurrentUserId();
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return NotFound();

            if (project.OwnerId == userId)
            {
                return Ok(ApiResponse<ProjectMemberDto>.Ok(new ProjectMemberDto
                {
                    ProjectId = projectId,
                    UserId = userId,
                    Role = Model.Domain.Enums.ProjectRole.Owner,
                    JoinedAt = project.CreatedAt
                }));
            }

            var member = await _projectRepository.GetMemberAsync(projectId, userId);
            if (member == null) return NotFound();

            return Ok(ApiResponse<ProjectMemberDto>.Ok(new ProjectMemberDto
            {
                ProjectId = member.ProjectId,
                UserId = member.UserId,
                Role = member.Role,
                JoinedAt = member.JoinedAt
            }));
        }

        [HttpPost]
        public async Task<IActionResult> AddMember(Guid projectId, [FromBody] AddProjectMemberRequestDto request)
        {
            await EnsureAdminAccessAsync(projectId);

            var existing = await _projectRepository.GetMemberAsync(projectId, request.UserId);
            if (existing != null) throw new ConflictException("User is already a member");

            var member = new ProjectMember(projectId, request.UserId, request.Role);
            await _projectRepository.AddMemberAsync(member);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Member added successfully"));
        }

        [HttpPatch("{userId:guid}/role")]
        public async Task<IActionResult> ChangeMemberRole(Guid projectId, Guid userId, [FromBody] ChangeProjectMemberRoleRequestDto request)
        {
            await EnsureAdminAccessAsync(projectId);

            var member = await _projectRepository.GetMemberAsync(projectId, userId);
            if (member == null) throw new NotFoundException("Member not found");

            member.ChangeRole(request.Role);
            await _unitOfWork.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(null, "Role updated successfully"));
        }

        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> RemoveMember(Guid projectId, Guid userId)
        {
            await EnsureAdminAccessAsync(projectId);

            var member = await _projectRepository.GetMemberAsync(projectId, userId);
            if (member == null) throw new NotFoundException("Member not found");

            _projectRepository.RemoveMember(member);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }
    }
}
