import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { TeamService } from '../../services/team.service';

@Component({
  selector: 'app-create-team',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-team.component.html',
  styleUrl: './create-team.component.css'
})
export class CreateTeamComponent implements OnInit {
  createTeamForm!: FormGroup;
  isSubmitting = false;
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private teamService: TeamService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.createTeamForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]]
    });
  }

  get nameControl() {
    return this.createTeamForm.get('name');
  }

  get descriptionControl() {
    return this.createTeamForm.get('description');
  }

  onSubmit(): void {
    if (this.createTeamForm.invalid) {
      this.createTeamForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.error = null;

    const { name, description } = this.createTeamForm.value;

    this.teamService.createTeam({ name, description: description || undefined }).subscribe({
      next: (team) => {
        // Navigate to the newly created team
        this.router.navigate(['/teams', team.id]);
      },
      error: (err) => {
        console.error('Error creating team:', err);
        this.error = err.error?.message || 'Failed to create team. Please try again.';
        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/teams']);
  }
}
