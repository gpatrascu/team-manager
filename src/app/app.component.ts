import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'Team Manager';
  message = '';
  userInfo: any = null;

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get('/api/GetMessage', { responseType: 'text' })
      .subscribe(data => this.message = data);
    
    this.http.get('/.auth/me')
      .subscribe(data => this.userInfo = data);
  }

  logout() {
    window.location.href = '/.auth/logout';
  }
}
