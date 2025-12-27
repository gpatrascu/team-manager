import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TeamService } from '../../services/team.service';
import { AuthService } from '../../services/auth.service';
import { Team } from '../../models/team.model';

@Component({
  selector: 'app-team-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './team-list.component.html',
  styleUrl: './team-list.component.css'
})
export class TeamListComponent implements OnInit {
  ownedTeams: Team[] = [];
  joinedTeams: Team[] = [];
  isLoading = true;
  error: string | null = null;
  currentUserId: string | null = null;

  constructor(
    private teamService: TeamService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.getCurrentUser()?.userId || null;
    this.loadTeams();
  }

  loadTeams(): void {
    this.isLoading = true;
    this.error = null;

    this.teamService.getMyTeams().subscribe({
      next: (teams) => {
        // Separate teams into owned (admin) and joined (member)
        this.ownedTeams = teams.filter(t => t.admins.includes(this.currentUserId!));
        this.joinedTeams = teams.filter(t =>
          !t.admins.includes(this.currentUserId!) &&
          t.members.some(m => m.userId === this.currentUserId && m.status === 'active')
        );
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading teams:', err);
        this.error = 'Failed to load teams. Please try again.';
        this.isLoading = false;
      }
    });
  }

  navigateToTeam(teamId: string): void {
    this.router.navigate(['/teams', teamId]);
  }

  navigateToCreateTeam(): void {
    this.router.navigate(['/teams/create']);
  }

  getActiveMemberCount(team: Team): number {
    return team.members.filter(m => m.status === 'active').length;
  }

  getRelativeTime(date: Date): string {
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const minutes = Math.floor(diff / (1000 * 60));

    if (days > 0) {
      return `${days} day${days > 1 ? 's' : ''} ago`;
    } else if (hours > 0) {
      return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    } else if (minutes > 0) {
      return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
    } else {
      return 'Just now';
    }
  }
}
