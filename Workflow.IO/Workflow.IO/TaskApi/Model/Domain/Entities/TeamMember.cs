using System;

namespace TaskApi.Model.Domain.Entities
{
    public class TeamMember
    {
        public Guid TeamMemberId { get; private set; }
        public Guid TeamId { get; private set; }
        public Guid UserId { get; private set; }
        public string Role { get; private set; } // "Lead" or "Member"
        public DateTime JoinedAt { get; private set; }

        public Team Team { get; private set; } = null!;

        private TeamMember()
        {
            Role = "Member";
        }

        public TeamMember(Guid teamId, Guid userId, string role = "Member")
        {
            TeamMemberId = Guid.NewGuid();
            TeamId = teamId;
            UserId = userId;
            Role = string.IsNullOrWhiteSpace(role) ? "Member" : role.Trim();
            JoinedAt = DateTime.UtcNow;
        }

        public TeamMember(Team team, Guid userId, string role = "Member")
        {
            TeamMemberId = Guid.NewGuid();
            Team = team;
            TeamId = team.TeamId;
            UserId = userId;
            Role = string.IsNullOrWhiteSpace(role) ? "Member" : role.Trim();
            JoinedAt = DateTime.UtcNow;
        }

        public void ChangeRole(string role)
        {
            Role = string.IsNullOrWhiteSpace(role) ? "Member" : role.Trim();
        }
    }
}
