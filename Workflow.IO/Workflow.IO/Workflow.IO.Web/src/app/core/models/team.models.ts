export interface TeamMemberResponse {
  teamMemberId: string;
  teamId: string;
  userId: string;
  role: string;
  joinedAt: string;
}

export interface TeamResponse {
  teamId: string;
  name: string;
  avatarUrl: string | null;
  leadId: string;
  visibility: string;
  description: string | null;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
  members: TeamMemberResponse[];
}

export interface CreateTeamRequest {
  name: string;
  avatarUrl?: string | null;
  leadId: string;
  visibility: string;
  description?: string | null;
}

export interface UpdateTeamRequest {
  name: string;
  avatarUrl?: string | null;
  leadId: string;
  visibility: string;
  description?: string | null;
}

export interface AddTeamMemberRequest {
  userId: string;
  role?: string;
}

export interface ChangeMemberRoleRequest {
  role: string;
}

export interface TransferTeamOwnershipRequest {
  newLeadId: string;
}

export interface StatusDistributionItem {
  statusName: string;
  count: number;
  percentage: number;
}

export interface TeamAnalytics {
  totalIssuesCount: number;
  completedIssuesCount: number;
  sprintCompletionRate: number;
  velocity: number;
  throughput: number;
  statusDistribution: StatusDistributionItem[];
}
