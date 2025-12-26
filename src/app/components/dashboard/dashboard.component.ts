import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService, UserInfo } from '../../services/auth.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  user$: Observable<UserInfo | null>;
  userName: string | null = null;
  userEmail: string | null = null;

  constructor(private authService: AuthService) {
    this.user$ = this.authService.user$;
  }

  ngOnInit(): void {
    // Get current user info
    const user = this.authService.getCurrentUser();
    if (user) {
      this.userName = this.authService.getUserName();
      this.userEmail = this.authService.getUserEmail();
    }
  }

  logout(): void {
    this.authService.logout();
  }
}
