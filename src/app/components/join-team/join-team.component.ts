import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TeamService } from '../../services/team.service';

@Component({
  selector: 'app-join-team',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './join-team.component.html',
  styleUrl: './join-team.component.css'
})
export class JoinTeamComponent implements OnInit {
  inviteToken: string | null = null;
  joinForm!: FormGroup;

  isValidating = true;
  isTokenValid = false;
  isSubmitting = false;

  teamName: string | null = null;
  invitedBy: string | null = null;
  memberCount: number = 0;

  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private teamService: TeamService
  ) {}

  ngOnInit(): void {
    // Extract token from URL
    this.inviteToken = this.route.snapshot.paramMap.get('token');

    if (!this.inviteToken) {
      this.error = 'Invalid invite link';
      this.isValidating = false;
      return;
    }

    // Initialize form
    this.joinForm = this.fb.group({
      nickname: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]]
    });

    // Note: In a full implementation, you would validate the token here by calling
    // the backend. For now, we'll assume it's valid and let the backend validate
    // on submission
    this.isTokenValid = true;
    this.isValidating = false;

    // In production, you might want to fetch team info here:
    // this.validateToken();
  }

  get nicknameControl() {
    return this.joinForm.get('nickname');
  }

  onSubmit(): void {
    if (this.joinForm.invalid || !this.inviteToken) {
      this.joinForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const { nickname } = this.joinForm.value;

    this.teamService.joinTeam({
      inviteToken: this.inviteToken,
      nickname: nickname
    }).subscribe({
      next: (team) => {
        // Navigate to the team detail page with a query param indicating pending status
        this.router.navigate(['/teams', team.id], {
          queryParams: { joined: 'pending' }
        });
      },
      error: (err) => {
        console.error('Error joining team:', err);

        // Handle specific error cases
        if (err.status === 400) {
          this.error = err.error?.message || 'Invalid or expired invite token';
        } else if (err.status === 409) {
          this.error = 'You are already a member of this team';
        } else {
          this.error = 'Failed to join team. Please try again.';
        }

        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/teams']);
  }
}
