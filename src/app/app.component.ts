import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'Team Manager';
  isLoading$;

  constructor(private authService: AuthService) {
    this.isLoading$ = this.authService.isLoading$;
  }

  ngOnInit() {
    // AuthService handles initialization automatically
    // User info is loaded in AuthService constructor
  }
}
