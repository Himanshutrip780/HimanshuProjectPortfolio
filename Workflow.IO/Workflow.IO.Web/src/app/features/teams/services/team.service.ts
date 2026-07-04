import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiHttpService } from '../../../core/services/api-http.service';
import {
  TeamResponse,
  CreateTeamRequest,
  UpdateTeamRequest,
  TeamMemberResponse,
  AddTeamMemberRequest,
  ChangeMemberRoleRequest,
  TransferTeamOwnershipRequest,
  TeamAnalytics
} from '../../../core/models/team.models';
import { TaskResponse, BoardResponse } from '../../../core/models/task.models';

@Injectable({ providedIn: 'root' })
export class TeamService {
  private readonly api = inject(ApiHttpService);

  getTeams(): Observable<TeamResponse[]> {
    return this.api.getList<TeamResponse>('/teams');
  }

  getTeamById(teamId: string): Observable<TeamResponse> {
    return this.api.get<TeamResponse>(`/teams/${teamId}`);
  }

  createTeam(request: CreateTeamRequest): Observable<TeamResponse> {
    return this.api.post<TeamResponse>('/teams', request);
  }

  updateTeam(teamId: string, request: UpdateTeamRequest): Observable<TeamResponse> {
    return this.api.put<TeamResponse>(`/teams/${teamId}`, request);
  }

  archiveTeam(teamId: string): Observable<void> {
    return this.api.delete(`/teams/${teamId}`);
  }

  restoreTeam(teamId: string): Observable<void> {
    return this.api.postCommand(`/teams/${teamId}/restore`);
  }

  addMember(teamId: string, request: AddTeamMemberRequest): Observable<TeamMemberResponse> {
    return this.api.post<TeamMemberResponse>(`/teams/${teamId}/members`, request);
  }

  removeMember(teamId: string, userId: string): Observable<void> {
    return this.api.delete(`/teams/${teamId}/members/${userId}`);
  }

  changeMemberRole(teamId: string, userId: string, request: ChangeMemberRoleRequest): Observable<TeamMemberResponse> {
    return this.api.put<TeamMemberResponse>(`/teams/${teamId}/members/${userId}/role`, request);
  }

  transferOwnership(teamId: string, request: TransferTeamOwnershipRequest): Observable<TeamResponse> {
    return this.api.put<TeamResponse>(`/teams/${teamId}/transfer-ownership`, request);
  }

  getTeamIssues(teamId: string): Observable<TaskResponse[]> {
    return this.api.getList<TaskResponse>(`/teams/${teamId}/issues`);
  }

  getTeamBoards(teamId: string): Observable<BoardResponse[]> {
    return this.api.getList<BoardResponse>(`/teams/${teamId}/boards`);
  }

  getTeamAnalytics(teamId: string): Observable<TeamAnalytics> {
    return this.api.get<TeamAnalytics>(`/teams/${teamId}/analytics`);
  }
}
