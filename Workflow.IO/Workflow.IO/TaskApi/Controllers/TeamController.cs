using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TaskApi.Data;
using TaskApi.Model.Domain.Entities;
using TaskApi.Model.Domain.Enums;
using TaskApi.Model.Dto;
using TaskApi.Repositories;
using Workflow.IO.Shared.Contracts;

namespace TaskApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IMapper _mapper;
        private readonly ITenantContext _tenantContext;

        public TeamController(ITaskRepository taskRepository, IMapper mapper, ITenantContext tenantContext)
        {
            _taskRepository = taskRepository;
            _mapper = mapper;
            _tenantContext = tenantContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            var orgId = _tenantContext.CurrentOrganizationId;
            if (orgId == null)
            {
                return BadRequest("No active workspace found.");
            }

            var team = new Team(
                request.Name,
                request.AvatarUrl,
                request.LeadId,
                request.Visibility,
                request.Description,
                orgId.Value
            );

            // Automatically add the Lead as a member with role "Lead"
            var leadMember = new TeamMember(team.TeamId, request.LeadId, "Lead");
            team.Members.Add(leadMember);

            // If the current user is not the lead, also add them as a member
            if (currentUserId != request.LeadId)
            {
                var creatorMember = new TeamMember(team.TeamId, currentUserId, "Member");
                team.Members.Add(creatorMember);
            }

            await _taskRepository.AddTeamAsync(team);
            await _taskRepository.SaveChangesAsync();

            var response = _mapper.Map<TeamResponseDto>(team);
            return Ok(ApiResponse<TeamResponseDto>.Ok(response, "Team created successfully."));
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var userId = GetCurrentUserId();
            var teams = await _taskRepository.GetVisibleTeamsAsync(userId);
            var response = _mapper.Map<IEnumerable<TeamResponseDto>>(teams);
            return Ok(ApiResponse<IEnumerable<TeamResponseDto>>.Ok(response));
        }

        [HttpGet("{teamId:guid}")]
        public async Task<IActionResult> GetTeamById(Guid teamId)
        {
            var userId = GetCurrentUserId();
            var team = await _taskRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            // Enforce private team visibility: user must be a member of the team to view it
            if (team.Visibility.Equals("Private", StringComparison.OrdinalIgnoreCase) &&
                !team.Members.Any(m => m.UserId == userId))
            {
                return Forbid();
            }

            var response = _mapper.Map<TeamResponseDto>(team);
            return Ok(ApiResponse<TeamResponseDto>.Ok(response));
        }

        [HttpPut("{teamId:guid}")]
        public async Task<IActionResult> UpdateTeam(Guid teamId, [FromBody] UpdateTeamRequestDto request)
        {
            var userId = GetCurrentUserId();
            var team = await _taskRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            // Only lead can update the team
            if (team.LeadId != userId)
            {
                return Forbid();
            }

            team.Update(
                request.Name,
                request.AvatarUrl,
                request.LeadId,
                request.Visibility,
                request.Description
            );

            // Sync Lead in TeamMembers list if they are not already there
            var hasLeadMember = team.Members.Any(m => m.UserId == request.LeadId);
            if (!hasLeadMember)
            {
                var newLeadMember = new TeamMember(team.TeamId, request.LeadId, "Lead");
                team.Members.Add(newLeadMember);
            }
            else
            {
                // Ensure their role is updated to Lead
                var leadMem = team.Members.First(m => m.UserId == request.LeadId);
                leadMem.ChangeRole("Lead");
            }

            await _taskRepository.SaveChangesAsync();

            var response = _mapper.Map<TeamResponseDto>(team);
            return Ok(ApiResponse<TeamResponseDto>.Ok(response, "Team updated successfully."));
        }

        [HttpDelete("{teamId:guid}")]
        public async Task<IActionResult> ArchiveTeam(Guid teamId)
        {
            var userId = GetCurrentUserId();
            var team = await _taskRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            if (team.LeadId != userId)
            {
                return Forbid();
            }

            team.Archive();
            await _taskRepository.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { teamId }, "Team archived successfully."));
        }

        [HttpPost("{teamId:guid}/restore")]
        public async Task<IActionResult> RestoreTeam(Guid teamId)
        {
            var userId = GetCurrentUserId();
            var team = await _taskRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            if (team.LeadId != userId)
            {
                return Forbid();
            }

            team.Restore();
            await _taskRepository.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { teamId }, "Team restored successfully."));
        }

        [HttpPost("{teamId:guid}/members")]
        public async Task<IActionResult> AddMember(Guid teamId, [FromBody] AddTeamMemberRequestDto request)
        {
            var userId = GetCurrentUserId();
            var team = await _taskRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            // Only team members can invite/add new members
            if (!team.Members.Any(m => m.UserId == userId))
            {
                return Forbid();
            }

            var existingMember = team.Members.FirstOrDefault(m => m.UserId == request.UserId);
            if (existingMember != null)
            {
                return BadRequest("User is already a member of the team.");
            }

            var member = new TeamMember(teamId, request.UserId, request.Role);
            await _taskRepository.AddTeamMemberAsync(member);
            await _taskRepository.SaveChangesAsync();

            var response = _mapper.Map<TeamMemberResponseDto>(member);
            return Ok(ApiResponse<TeamMemberResponseDto>.Ok(response, "Member added successfully."));
        }

        [HttpDelete("{teamId:guid}/members/{memberUserId:guid}")]
        public async Task<IActionResult> RemoveMember(Guid teamId, Guid memberUserId)
        {
            var currentUserId = GetCurrentUserId();
            var team = await _taskRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            // Users can leave themselves, or the lead can remove members
            if (currentUserId != memberUserId && team.LeadId != currentUserId)
            {
                return Forbid();
            }

            var member = team.Members.FirstOrDefault(m => m.UserId == memberUserId);
            if (member == null)
            {
                return NotFound();
            }

            // Cannot remove the lead without transferring ownership first
            if (team.LeadId == memberUserId)
            {
                return BadRequest("Cannot remove the team lead. Transfer ownership first.");
            }

            await _taskRepository.RemoveTeamMemberAsync(member);
            await _taskRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{teamId:guid}/members/{memberUserId:guid}/role")]
        public async Task<IActionResult> ChangeMemberRole(Guid teamId, Guid memberUserId, [FromBody] ChangeMemberRoleRequestDto request)
        {
            var currentUserId = GetCurrentUserId();
            var team = await _taskRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            if (team.LeadId != currentUserId)
            {
                return Forbid();
            }

            var member = team.Members.FirstOrDefault(m => m.UserId == memberUserId);
            if (member == null)
            {
                return NotFound();
            }

            member.ChangeRole(request.Role);
            await _taskRepository.SaveChangesAsync();

            return Ok(ApiResponse<TeamMemberResponseDto>.Ok(_mapper.Map<TeamMemberResponseDto>(member), "Role updated successfully."));
        }

        [HttpPut("{teamId:guid}/transfer-ownership")]
        public async Task<IActionResult> TransferOwnership(Guid teamId, [FromBody] TransferTeamOwnershipRequestDto request)
        {
            var currentUserId = GetCurrentUserId();
            var team = await _taskRepository.GetTeamByIdAsync(teamId);

            if (team == null)
            {
                return NotFound();
            }

            if (team.LeadId != currentUserId)
            {
                return Forbid();
            }

            var newLeadMember = team.Members.FirstOrDefault(m => m.UserId == request.NewLeadId);
            if (newLeadMember == null)
            {
                return BadRequest("New lead must be an existing team member.");
            }

            // Demote current lead to Member, promote new lead
            var currentLeadMember = team.Members.FirstOrDefault(m => m.UserId == currentUserId);
            currentLeadMember?.ChangeRole("Member");

            newLeadMember.ChangeRole("Lead");
            team.SetLead(request.NewLeadId);

            await _taskRepository.SaveChangesAsync();

            var response = _mapper.Map<TeamResponseDto>(team);
            return Ok(ApiResponse<TeamResponseDto>.Ok(response, "Ownership transferred successfully."));
        }

        [HttpGet("{teamId:guid}/issues")]
        public async Task<IActionResult> GetTeamIssues(Guid teamId)
        {
            var tasks = await _taskRepository.GetAllTasksAsync();
            var teamTasks = tasks.Where(t => t.TeamId == teamId);
            var response = _mapper.Map<IEnumerable<TaskResponseDto>>(teamTasks);
            return Ok(ApiResponse<IEnumerable<TaskResponseDto>>.Ok(response));
        }

        [HttpGet("{teamId:guid}/boards")]
        public async Task<IActionResult> GetTeamBoards(Guid teamId)
        {
            var tasks = await _taskRepository.GetAllTasksAsync();
            var projectIds = tasks.Where(t => t.TeamId == teamId).Select(t => t.ProjectId).Distinct().ToList();

            var boards = new List<Board>();
            foreach (var projId in projectIds)
            {
                var board = await _taskRepository.GetBoardByProjectIdAsync(projId);
                if (board != null)
                {
                    boards.Add(board);
                }
            }

            var response = _mapper.Map<IEnumerable<BoardResponseDto>>(boards);
            return Ok(ApiResponse<IEnumerable<BoardResponseDto>>.Ok(response));
        }

        [HttpGet("{teamId:guid}/analytics")]
        public async Task<IActionResult> GetTeamAnalytics(Guid teamId)
        {
            var tasks = await _taskRepository.GetAllTasksAsync();
            var teamTasks = tasks.Where(t => t.TeamId == teamId).ToList();

            var totalCount = teamTasks.Count;
            var completedCount = teamTasks.Count(t => t.Status == Model.Domain.Enums.TaskStatus.Done);

            // Compute Sprint Completion Rate (based on tasks in Sprints)
            double completionRate = 0;
            var sprintIds = teamTasks.Where(t => t.SprintId.HasValue).Select(t => t.SprintId!.Value).Distinct().ToList();
            var sprintsCompletedTasks = 0;
            var sprintsTotalTasks = 0;

            foreach (var sprintId in sprintIds)
            {
                var sprint = await _taskRepository.GetSprintByIdAsync(sprintId);
                if (sprint != null && sprint.Status == SprintStatus.Completed)
                {
                    var sprintTasks = teamTasks.Where(t => t.SprintId == sprintId).ToList();
                    sprintsTotalTasks += sprintTasks.Count;
                    sprintsCompletedTasks += sprintTasks.Count(t => t.Status == Model.Domain.Enums.TaskStatus.Done);
                }
            }

            if (sprintsTotalTasks > 0)
            {
                completionRate = Math.Round((double)sprintsCompletedTasks * 100 / sprintsTotalTasks, 1);
            }

            // Velocity: average tasks completed in completed sprints
            double velocity = 0;
            var completedSprintsCount = 0;
            var completedSprintsTotalDone = 0;

            foreach (var sprintId in sprintIds)
            {
                var sprint = await _taskRepository.GetSprintByIdAsync(sprintId);
                if (sprint != null && sprint.Status == SprintStatus.Completed)
                {
                    completedSprintsCount++;
                    completedSprintsTotalDone += teamTasks.Count(t => t.SprintId == sprintId && t.Status == Model.Domain.Enums.TaskStatus.Done);
                }
            }

            if (completedSprintsCount > 0)
            {
                velocity = Math.Round((double)completedSprintsTotalDone / completedSprintsCount, 1);
            }

            // Throughput: completed tasks per week over the last 30 days
            double throughput = 0;
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var recentlyCompleted = teamTasks.Count(t => t.Status == Model.Domain.Enums.TaskStatus.Done && t.UpdatedAt >= thirtyDaysAgo);
            throughput = Math.Round((double)recentlyCompleted / 4.28, 1); // 4.28 weeks in 30 days

            // Status distribution
            var statusDistribution = teamTasks
                .GroupBy(t => t.Status)
                .Select(g => new StatusDistributionItem
                {
                    StatusName = g.Key.ToString(),
                    Count = g.Count(),
                    Percentage = totalCount == 0 ? 0 : Math.Round((double)g.Count() * 100 / totalCount, 1)
                })
                .ToList();

            var response = new TeamAnalyticsDto
            {
                TotalIssuesCount = totalCount,
                CompletedIssuesCount = completedCount,
                SprintCompletionRate = completionRate,
                Velocity = velocity,
                Throughput = throughput,
                StatusDistribution = statusDistribution
            };

            return Ok(ApiResponse<TeamAnalyticsDto>.Ok(response));
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException();
            }
            return Guid.Parse(userIdClaim);
        }
    }
}
