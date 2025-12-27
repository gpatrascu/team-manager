import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  Team,
  TeamMember,
  CreateTeamRequest,
  GenerateInviteTokenRequest,
  GenerateInviteTokenResult,
  JoinTeamRequest
} from '../models/team.model';

@Injectable({
  providedIn: 'root'
})
export class TeamService {
  private readonly API_BASE = '/api';

  constructor(private http: HttpClient) {}

  /**
   * Get all teams where the user is either admin or active member
   */
  getMyTeams(): Observable<Team[]> {
    return this.http.get<Team[]>(`${this.API_BASE}/teams/my-teams`).pipe(
      map(teams => teams.map(team => this.parseTeamDates(team)))
    );
  }

  /**
   * Get a specific team by ID
   * Includes authorization check on backend
   */
  getTeam(teamId: string): Observable<Team> {
    return this.http.get<Team>(`${this.API_BASE}/teams/${teamId}`).pipe(
      map(team => this.parseTeamDates(team))
    );
  }

  /**
   * Create a new team
   * The requesting user becomes the admin
   */
  createTeam(request: CreateTeamRequest): Observable<Team> {
    return this.http.post<Team>(`${this.API_BASE}/teams`, request).pipe(
      map(team => this.parseTeamDates(team))
    );
  }

  /**
   * Generate an invite token for a team (admin only)
   * @param teamId The team ID
   * @param request Optional request with expiry hours (default: 168 hours / 7 days)
   */
  generateInviteToken(teamId: string, request: GenerateInviteTokenRequest = {}): Observable<GenerateInviteTokenResult> {
    return this.http.post<GenerateInviteTokenResult>(
      `${this.API_BASE}/teams/${teamId}/invite-token`,
      request
    ).pipe(
      map(result => ({
        ...result,
        expiryDate: new Date(result.expiryDate)
      }))
    );
  }

  /**
   * Join a team using an invite token
   * Creates a pending membership that requires admin approval
   */
  joinTeam(request: JoinTeamRequest): Observable<Team> {
    return this.http.post<Team>(`${this.API_BASE}/teams/join`, request).pipe(
      map(team => this.parseTeamDates(team))
    );
  }

  /**
   * Get pending members for a team (admin only)
   */
  getPendingMembers(teamId: string): Observable<TeamMember[]> {
    return this.http.get<TeamMember[]>(`${this.API_BASE}/teams/${teamId}/members/pending`).pipe(
      map(members => members.map(member => this.parseMemberDates(member)))
    );
  }

  /**
   * Approve a pending member (admin only)
   * Changes member status from 'pending' to 'active'
   */
  approveMember(teamId: string, memberId: string): Observable<Team> {
    return this.http.post<Team>(
      `${this.API_BASE}/teams/${teamId}/members/${memberId}/approve`,
      {}
    ).pipe(
      map(team => this.parseTeamDates(team))
    );
  }

  /**
   * Reject a pending member (admin only)
   * Changes member status from 'pending' to 'inactive'
   */
  rejectMember(teamId: string, memberId: string): Observable<Team> {
    return this.http.post<Team>(
      `${this.API_BASE}/teams/${teamId}/members/${memberId}/reject`,
      {}
    ).pipe(
      map(team => this.parseTeamDates(team))
    );
  }

  /**
   * Parse date strings to Date objects for a team
   */
  private parseTeamDates(team: any): Team {
    return {
      ...team,
      createdAt: new Date(team.createdAt),
      updatedAt: new Date(team.updatedAt),
      inviteTokenExpiry: team.inviteTokenExpiry ? new Date(team.inviteTokenExpiry) : undefined,
      members: team.members.map((m: any) => this.parseMemberDates(m))
    };
  }

  /**
   * Parse date strings to Date objects for a team member
   */
  private parseMemberDates(member: any): TeamMember {
    return {
      ...member,
      joinedAt: new Date(member.joinedAt),
      approvedAt: member.approvedAt ? new Date(member.approvedAt) : undefined
    };
  }
}
