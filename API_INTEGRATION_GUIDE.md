# API Integration Guide with Facebook Authentication

This guide shows how to call your backend APIs with authenticated requests using the AuthService.

## Getting Authenticated User Info in Components

### Basic Usage

```typescript
import { Component, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-my-component',
  template: `
    <div *ngIf="userName">
      <p>Welcome, {{ userName }}</p>
      <p>Email: {{ userEmail }}</p>
    </div>
  `
})
export class MyComponent implements OnInit {
  userName: string | null = null;
  userEmail: string | null = null;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    this.userName = this.authService.getUserName();
    this.userEmail = this.authService.getUserEmail();
  }
}
```

### Using Observables (Reactive)

```typescript
import { Component } from '@angular/core';
import { AuthService, UserInfo } from './services/auth.service';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-user-profile',
  template: `
    <div *ngIf="user$ | async as user">
      <h2>{{ user.userDetails }}</h2>
      <div *ngFor="let claim of user.claims">
        <p>{{ claim.typ }}: {{ claim.val }}</p>
      </div>
    </div>
  `
})
export class UserProfileComponent {
  user$: Observable<UserInfo | null>;

  constructor(private authService: AuthService) {
    this.user$ = this.authService.user$;
  }
}
```

## Calling Protected APIs

### Example: API to Get User Teams

The authenticated user's info is available on the backend through Azure SWA's auth headers.

#### Backend Example (C# .NET)

```csharp
[HttpGet("teams")]
public async Task<IActionResult> GetUserTeams()
{
    // Get authenticated user from header
    var clientPrincipal = HttpContext.User;
    var userId = clientPrincipal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    var email = clientPrincipal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

    if (string.IsNullOrEmpty(userId))
    {
        return Unauthorized("User not authenticated");
    }

    // Fetch teams for this user from database
    var teams = await _database.Teams
        .Where(t => t.UserId == userId)
        .ToListAsync();

    return Ok(teams);
}
```

#### Frontend Call from Component

```typescript
import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './services/auth.service';

interface Team {
  id: string;
  name: string;
  description: string;
}

@Component({
  selector: 'app-teams',
  template: `
    <div>
      <h2>Your Teams</h2>
      <div *ngIf="teams$ | async as teams">
        <div *ngFor="let team of teams">
          <h3>{{ team.name }}</h3>
          <p>{{ team.description }}</p>
        </div>
      </div>
      <p *ngIf="error">{{ error }}</p>
    </div>
  `
})
export class TeamsComponent implements OnInit {
  teams$ = this.http.get<Team[]>('/api/teams');
  error: string | null = null;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit() {
    // Verify user is authenticated before fetching
    if (!this.authService.isAuthenticated()) {
      this.error = 'User not authenticated';
    }
  }
}
```

## HTTP Interceptor Handling

The `authInterceptor` automatically handles:

1. **Authentication Errors**: Redirects to login on 401/403
2. **Auth Endpoints**: Skips interceptor for `/.auth/*` routes
3. **Error Logging**: Logs authentication errors for debugging

### Using the Interceptor

No special configuration needed - it's already configured in `app.config.ts`:

```typescript
provideHttpClient(
  withInterceptors([authInterceptor])
)
```

All HTTP requests automatically get the authentication cookie from the browser.

## API Route Examples

### Public Routes (No Authentication)

```json
{
  "route": "/api/public/*",
  "allowedRoles": ["anonymous"]
}
```

Backend:
```csharp
[HttpGet("public/info")]
public IActionResult GetPublicInfo()
{
    return Ok(new { message = "This is public" });
}
```

### Protected Routes (Authenticated Only)

```json
{
  "route": "/api/teams/*",
  "allowedRoles": ["authenticated"]
}
```

Azure SWA automatically blocks unauthenticated requests at the routing level.

## Using User Claims in API Calls

### Send User Email in Request

```typescript
import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './services/auth.service';

interface UpdateProfileRequest {
  name: string;
  email: string;
}

@Component({
  selector: 'app-profile-settings'
})
export class ProfileSettingsComponent {
  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  updateProfile(name: string) {
    const email = this.authService.getUserEmail();
    const request: UpdateProfileRequest = {
      name,
      email: email || ''
    };

    this.http.post('/api/profile/update', request).subscribe(
      (response) => console.log('Profile updated', response),
      (error) => console.error('Update failed', error)
    );
  }
}
```

### Backend Validates and Uses User Claims

```csharp
[HttpPost("profile/update")]
public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
{
    // Get authenticated user from header
    var authenticatedUserId = HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

    // Validate that the request comes from authenticated user
    if (string.IsNullOrEmpty(authenticatedUserId))
    {
        return Unauthorized();
    }

    // Update user profile in database
    var user = await _database.Users.FindAsync(authenticatedUserId);
    if (user == null)
    {
        return NotFound();
    }

    user.Name = request.Name;
    user.Email = request.Email;
    await _database.SaveChangesAsync();

    return Ok(new { message = "Profile updated successfully" });
}
```

## Error Handling

### Handle 401/403 Errors

The interceptor handles these automatically, but you can also catch them:

```typescript
import { Component } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-data-component'
})
export class DataComponent {
  constructor(private http: HttpClient) {}

  fetchData() {
    this.http.get('/api/protected-data')
      .pipe(
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401) {
            console.log('Not authenticated');
          } else if (error.status === 403) {
            console.log('Access denied');
          } else {
            console.error('Error:', error.message);
          }
          return of(null);
        })
      )
      .subscribe(data => console.log(data));
  }
}
```

## Upload Files with Authentication

```typescript
import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-file-upload'
})
export class FileUploadComponent {
  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  uploadFile(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('userId', this.authService.getCurrentUser()?.userId || '');

    this.http.post('/api/files/upload', formData).subscribe(
      (response) => console.log('Upload successful', response),
      (error) => console.error('Upload failed', error)
    );
  }
}
```

Backend:
```csharp
[HttpPost("files/upload")]
public async Task<IActionResult> UploadFile(IFormFile file, string userId)
{
    // Verify authenticated user matches the userId
    var authenticatedUserId = HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

    if (authenticatedUserId != userId)
    {
        return Forbid();
    }

    // Handle file upload
    var fileInfo = await SaveFile(file, userId);
    return Ok(fileInfo);
}
```

## Testing APIs with Authentication

### Using Browser DevTools

1. Open Network tab
2. Login via Facebook
3. Check request headers for authentication cookie
4. Make API calls and verify the cookie is sent

### Manual Testing with cURL

```bash
# Get authentication cookie
curl -X GET "http://localhost:4200/.auth/me" -c cookies.txt

# Use cookie in API request
curl -X GET "http://localhost:4200/api/teams" -b cookies.txt
```

### Postman Testing

1. First, manually login to your app via browser
2. Copy the authentication cookie from DevTools
3. In Postman, add the cookie to the Cookie header
4. Make requests to your API endpoints

## Best Practices

1. **Always Check Authentication**: Use `authService.isAuthenticated()` before making protected API calls
2. **Handle Errors Gracefully**: Implement proper error handling for failed requests
3. **Use Typed Responses**: Define interfaces for API responses (e.g., `Team[]`)
4. **Validate on Backend**: Never trust client-side data; always validate on the backend
5. **Use User Claims**: Extract user info from authenticated session on backend, not from request body
6. **Secure Sensitive Operations**: Protect DELETE/PATCH operations with role checks

## Common API Patterns

### CRUD Operations

```typescript
// Create
createTeam(name: string) {
  return this.http.post('/api/teams', { name });
}

// Read
getTeams() {
  return this.http.get('/api/teams');
}

// Update
updateTeam(id: string, name: string) {
  return this.http.patch(`/api/teams/${id}`, { name });
}

// Delete
deleteTeam(id: string) {
  return this.http.delete(`/api/teams/${id}`);
}
```

### Pagination

```typescript
getTeams(page: number = 1, pageSize: number = 10) {
  const params = new HttpParams()
    .set('page', page.toString())
    .set('pageSize', pageSize.toString());

  return this.http.get('/api/teams', { params });
}
```

### Filtering and Search

```typescript
searchTeams(query: string) {
  return this.http.get('/api/teams/search', {
    params: { q: query }
  });
}
```
