import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TeamService } from '../../services/team.service';
import { AuthService } from '../../services/auth.service';
import { Team, TeamMember } from '../../models/team.model';

@Component({
  selector: 'app-team-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './team-detail.component.html',
  styleUrl: './team-detail.component.css'
})
export class TeamDetailComponent implements OnInit {
  team: Team | null = null;
  pendingMembers: TeamMember[] = [];
  activeMembers: TeamMember[] = [];
  isLoading = true;
  error: string | null = null;
  currentUserId: string | null = null;

  // User role flags
  isAdmin = false;
  isActiveMember = false;
  isPendingMember = false;

  // Invite token state
  inviteToken: string | null = null;
  inviteTokenExpiry: Date | null = null;
  isGeneratingToken = false;
  tokenCopied = false;

  // Action states
  processingMemberId: string | null = null;

  // Make window accessible to template
  window = window;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private teamService: TeamService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.getCurrentUser()?.userId || null;
    const teamId = this.route.snapshot.paramMap.get('id');

    if (teamId) {
      this.loadTeam(teamId);
    } else {
      this.error = 'Invalid team ID';
      this.isLoading = false;
    }
  }

  loadTeam(teamId: string): void {
    this.isLoading = true;
    this.error = null;

    this.teamService.getTeam(teamId).subscribe({
      next: (team) => {
        this.team = team;
        this.updateUserRole();
        this.categorizeMembers();

        // Load pending members if admin
        if (this.isAdmin) {
          this.loadPendingMembers(teamId);
        }

        // Set invite token if available
        if (team.inviteToken) {
          this.inviteToken = team.inviteToken;
          this.inviteTokenExpiry = team.inviteTokenExpiry || null;
        }

        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error loading team:', err);
        this.error = 'Failed to load team details. Please try again.';
        this.isLoading = false;
      }
    });
  }

  loadPendingMembers(teamId: string): void {
    this.teamService.getPendingMembers(teamId).subscribe({
      next: (members) => {
        this.pendingMembers = members;
      },
      error: (err) => {
        console.error('Error loading pending members:', err);
      }
    });
  }

  updateUserRole(): void {
    if (!this.team || !this.currentUserId) return;

    this.isAdmin = this.team.admins.includes(this.currentUserId);

    const currentMember = this.team.members.find(m => m.userId === this.currentUserId);
    if (currentMember) {
      this.isActiveMember = currentMember.status === 'active';
      this.isPendingMember = currentMember.status === 'pending';
    }
  }

  categorizeMembers(): void {
    if (!this.team) return;

    this.activeMembers = this.team.members.filter(m => m.status === 'active');
    // Pending members loaded separately for admins
  }

  generateInviteToken(): void {
    if (!this.team || !this.isAdmin) return;

    this.isGeneratingToken = true;

    this.teamService.generateInviteToken(this.team.id).subscribe({
      next: (result) => {
        this.inviteToken = result.inviteToken;
        this.inviteTokenExpiry = result.expiryDate;
        this.isGeneratingToken = false;
      },
      error: (err) => {
        console.error('Error generating invite token:', err);
        alert('Failed to generate invite token. Please try again.');
        this.isGeneratingToken = false;
      }
    });
  }

  copyInviteLink(): void {
    if (!this.inviteToken) return;

    const inviteUrl = `${window.location.origin}/join/${this.inviteToken}`;
    navigator.clipboard.writeText(inviteUrl).then(() => {
      this.tokenCopied = true;
      setTimeout(() => {
        this.tokenCopied = false;
      }, 2000);
    }).catch(err => {
      console.error('Failed to copy:', err);
      alert('Failed to copy link to clipboard');
    });
  }

  approveMember(memberId: string): void {
    if (!this.team || !this.isAdmin) return;

    this.processingMemberId = memberId;

    this.teamService.approveMember(this.team.id, memberId).subscribe({
      next: (updatedTeam) => {
        this.team = updatedTeam;
        this.categorizeMembers();
        this.loadPendingMembers(this.team.id);
        this.processingMemberId = null;
      },
      error: (err) => {
        console.error('Error approving member:', err);
        alert('Failed to approve member. Please try again.');
        this.processingMemberId = null;
      }
    });
  }

  rejectMember(memberId: string): void {
    if (!this.team || !this.isAdmin) return;

    if (!confirm('Are you sure you want to reject this member?')) {
      return;
    }

    this.processingMemberId = memberId;
    const teamId = this.team.id;

    this.teamService.rejectMember(teamId, memberId).subscribe({
      next: () => {
        this.loadPendingMembers(teamId);
        this.processingMemberId = null;
      },
      error: (err) => {
        console.error('Error rejecting member:', err);
        alert('Failed to reject member. Please try again.');
        this.processingMemberId = null;
      }
    });
  }

  isTokenExpired(): boolean {
    if (!this.inviteTokenExpiry) return false;
    return new Date() > this.inviteTokenExpiry;
  }

  getTokenExpiryText(): string {
    if (!this.inviteTokenExpiry) return '';

    const now = new Date();
    const expiry = this.inviteTokenExpiry;
    const diff = expiry.getTime() - now.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));

    if (diff < 0) {
      return 'Expired';
    } else if (days > 0) {
      return `Expires in ${days} day${days > 1 ? 's' : ''}`;
    } else if (hours > 0) {
      return `Expires in ${hours} hour${hours > 1 ? 's' : ''}`;
    } else {
      return 'Expires soon';
    }
  }

  getRelativeTime(date: Date): string {
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor(diff / (1000 * 60 * 60));

    if (days > 0) {
      return `${days} day${days > 1 ? 's' : ''} ago`;
    } else if (hours > 0) {
      return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    } else {
      return 'Just now';
    }
  }

  goBack(): void {
    this.router.navigate(['/teams']);
  }
}
