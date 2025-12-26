# AuthService API Reference

Complete API documentation for the AuthService used in your Angular application.

## Overview

The AuthService manages Facebook authentication through Azure Static Web Apps' built-in provider. It provides reactive user state management via RxJS observables.

## Service Location

```typescript
import { AuthService } from './services/auth.service';
```

## Injection

```typescript
constructor(private authService: AuthService) {}
```

## Public Observables

### `user$: Observable<UserInfo | null>`

Emits the current authenticated user or null if not authenticated.

**Type**: `Observable<UserInfo | null>`

**Usage**:
```typescript
export class MyComponent {
  user$ = this.authService.user$;

  constructor(private authService: AuthService) {}
}
```

**Template**:
```html
<div *ngIf="user$ | async as user">
  <p>Logged in as: {{ user.userDetails }}</p>
</div>
```

---

### `isAuthenticated$: Observable<boolean>`

Emits true if user is authenticated, false otherwise. Updates whenever authentication status changes.

**Type**: `Observable<boolean>`

**Usage**:
```typescript
isLoggedIn$ = this.authService.isAuthenticated$;
```

**Template**:
```html
<button *ngIf="!(isLoggedIn$ | async)">Login</button>
<button *ngIf="isLoggedIn$ | async">Logout</button>
```

---

### `isLoading$: Observable<boolean>`

Emits true while user authentication is being determined (on app startup). Emits false once auth status is known.

**Type**: `Observable<boolean>`

**Usage**:
```typescript
isLoading$ = this.authService.isLoading$;
```

**Template**:
```html
<div *ngIf="isLoading$ | async">Loading user info...</div>
<div *ngIf="!(isLoading$ | async)">Authentication check complete</div>
```

---

## Public Methods

### `getUserInfo(): Observable<UserInfo | null>`

Fetches the current user information from the Azure SWA auth endpoint (`/.auth/me`).

**Returns**: `Observable<UserInfo | null>`

**Parameters**: None

**Example**:
```typescript
this.authService.getUserInfo().subscribe(
  (user) => {
    if (user) {
      console.log('User ID:', user.userId);
      console.log('Email:', user.userDetails);
    }
  },
  (error) => console.error('Failed to fetch user:', error)
);
```

**Note**: This is called automatically on service initialization. You typically don't need to call this manually.

---

### `getCurrentUser(): UserInfo | null`

Gets the current user synchronously (non-observable). Useful when you need the user data in the current synchronous context.

**Returns**: `UserInfo | null`

**Parameters**: None

**Example**:
```typescript
const user = this.authService.getCurrentUser();
if (user) {
  console.log('Current user:', user.userDetails);
} else {
  console.log('User not authenticated');
}
```

**Warning**: This returns null until the initial authentication check completes. Check `isLoading$` first if you need to verify it's ready.

---

### `isAuthenticated(): boolean`

Synchronously checks if the user is currently authenticated.

**Returns**: `boolean`

**Parameters**: None

**Example**:
```typescript
if (this.authService.isAuthenticated()) {
  this.fetchUserData();
} else {
  this.redirectToLogin();
}
```

---

### `getUserEmail(): string | null`

Extracts the user's email address from their claims.

**Returns**: `string | null`

**Parameters**: None

**Example**:
```typescript
const email = this.authService.getUserEmail();
if (email) {
  console.log('User email:', email);
}
```

**Note**: The email comes from the Facebook user data as a claim. This returns null if user is not authenticated or email claim is not available.

---

### `getUserName(): string | null`

Extracts the user's name from their claims.

**Returns**: `string | null`

**Parameters**: None

**Example**:
```typescript
const name = this.authService.getUserName();
console.log('Welcome,', name || 'Guest');
```

**Fallback**: If the name claim is not available, falls back to `userDetails`.

---

### `login(): void`

Initiates the Facebook login flow. Redirects the user to Azure SWA's Facebook OAuth endpoint.

**Returns**: `void`

**Parameters**: None

**Example**:
```typescript
export class LoginComponent {
  constructor(private authService: AuthService) {}

  handleLogin() {
    this.authService.login();
    // User will be redirected to Facebook login page
  }
}
```

**Behavior**:
1. User is redirected to `/.auth/login/facebook`
2. Facebook login page is shown
3. After successful authentication, user is redirected to `/dashboard`
4. Session cookie is automatically set by Azure SWA

---

### `logout(): void`

Logs out the user and clears the session. Redirects to the home page.

**Returns**: `void`

**Parameters**: None

**Example**:
```typescript
export class NavbarComponent {
  constructor(private authService: AuthService) {}

  handleLogout() {
    this.authService.logout();
    // User session is cleared and user is redirected to home page
  }
}
```

**Behavior**:
1. User is redirected to `/.auth/logout`
2. Azure SWA clears the session cookie
3. User is redirected to `/`
4. All authentication state is reset

---

### `refreshUserInfo(): Observable<UserInfo | null>`

Explicitly refreshes the user information from the server. Useful after user updates their profile.

**Returns**: `Observable<UserInfo | null>`

**Parameters**: None

**Example**:
```typescript
export class ProfileComponent {
  constructor(private authService: AuthService) {}

  onProfileUpdated() {
    this.authService.refreshUserInfo().subscribe(
      (user) => console.log('User info refreshed:', user),
      (error) => console.error('Failed to refresh:', error)
    );
  }
}
```

**Note**: This automatically updates the internal `user$` and `isAuthenticated$` observables.

---

## Data Types

### `UserInfo` Interface

```typescript
interface UserInfo {
  identityProvider: string;        // "facebook"
  userId: string;                  // Unique Facebook user ID
  userDetails: string;             // User email or name
  claims: Array<{
    typ: string;                   // Claim type (e.g., email, name)
    val: string;                   // Claim value
  }>;
}
```

**Example**:
```json
{
  "identityProvider": "facebook",
  "userId": "1234567890",
  "userDetails": "john.doe@example.com",
  "claims": [
    {
      "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress",
      "val": "john.doe@example.com"
    },
    {
      "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
      "val": "John Doe"
    }
  ]
}
```

---

## Common Usage Patterns

### Pattern 1: Display User Info in Template

```typescript
export class NavbarComponent {
  user$ = this.authService.user$;
  userName$ = this.user$.pipe(
    map(user => this.authService.getUserName())
  );

  constructor(private authService: AuthService) {}
}
```

```html
<nav>
  <div *ngIf="(user$ | async) as user">
    <span>Welcome, {{ user.userDetails }}</span>
    <button (click)="logout()">Logout</button>
  </div>
  <div *ngIf="!(user$ | async)">
    <button (click)="login()">Login</button>
  </div>
</nav>
```

---

### Pattern 2: Conditional Component Rendering

```typescript
export class DashboardComponent {
  isAuthenticated$ = this.authService.isAuthenticated$;

  constructor(private authService: AuthService) {}
}
```

```html
<div *ngIf="isAuthenticated$ | async; else notLoggedIn">
  <!-- Protected content -->
</div>

<ng-template #notLoggedIn>
  <p>Please log in to continue</p>
</ng-template>
```

---

### Pattern 3: Use in Component Logic

```typescript
export class DataComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private http: HttpClient
  ) {}

  ngOnInit() {
    if (this.authService.isAuthenticated()) {
      const userId = this.authService.getCurrentUser()?.userId;
      this.fetchUserData(userId);
    } else {
      console.log('User not authenticated');
    }
  }

  private fetchUserData(userId: string | undefined) {
    // Fetch data based on user ID
  }
}
```

---

### Pattern 4: Handle Async Authentication Check

```typescript
export class AppComponent implements OnInit {
  isLoading$ = this.authService.isLoading$;
  isReady$ = this.isLoading$.pipe(
    map(loading => !loading),
    startWith(false)
  );

  constructor(private authService: AuthService) {}
}
```

```html
<div *ngIf="isReady$ | async; else loading">
  <!-- App content -->
</div>

<ng-template #loading>
  <p>Initializing authentication...</p>
</ng-template>
```

---

### Pattern 5: Logout and Redirect

```typescript
export class AccountComponent {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  logout() {
    // Note: logout() redirects immediately, so no need to manually navigate
    this.authService.logout();
  }
}
```

---

## Error Handling

### Handling Failed User Info Fetch

The AuthService automatically handles errors when fetching user info:

```typescript
// Inside AuthService initialization
this.getUserInfo().subscribe(
  (user) => {
    // Success
  },
  (error) => {
    // Error is caught and logged
    console.log('User not authenticated', error);
  }
);
```

If you need to handle errors in your component:

```typescript
this.authService.getUserInfo().subscribe(
  (user) => console.log('User:', user),
  (error) => console.error('Fetch failed:', error),
  () => console.log('Request completed')
);
```

---

## Lifecycle

### Service Initialization Flow

1. **Service created**: `constructor()` runs
2. **`initializeAuth()` called**: Starts fetching user info
3. **`isLoading$` = true**: App shows loading indicator
4. **HTTP request to `/.auth/me`**: Asks Azure SWA for user info
5. **Response received**:
   - If authenticated: `user$` and `isAuthenticated$` updated
   - If not authenticated: `user$` = null, `isAuthenticated$` = false
6. **`isLoading$` = false**: App ready to use

### Timeline

```
App Start
    |
    v
AuthService instantiated
    |
    v
initializeAuth() called
    |
    v
isLoading$ = true ────────> App shows loading screen
    |
    v
HTTP GET /.auth/me
    |
    v
Response received
    |
    ├─ Has user? -> Update observables
    |
    v
isLoading$ = false ───────> App renders based on auth status
```

---

## Testing

### Mock AuthService for Tests

```typescript
import { of } from 'rxjs';

const mockAuthService = {
  user$: of(null),
  isAuthenticated$: of(false),
  isLoading$: of(false),
  getUserInfo: jasmine.createSpy().and.returnValue(of(null)),
  getCurrentUser: jasmine.createSpy().and.returnValue(null),
  isAuthenticated: jasmine.createSpy().and.returnValue(false),
  getUserEmail: jasmine.createSpy().and.returnValue(null),
  getUserName: jasmine.createSpy().and.returnValue(null),
  login: jasmine.createSpy(),
  logout: jasmine.createSpy(),
  refreshUserInfo: jasmine.createSpy().and.returnValue(of(null))
};

describe('MyComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();
  });

  it('should show login button when not authenticated', () => {
    // Test implementation
  });
});
```

---

## Performance Tips

1. **Use `async` pipe**: Automatically unsubscribes to prevent memory leaks
   ```html
   <div *ngIf="user$ | async">Welcome</div>
   ```

2. **Share observables**: Avoid making multiple subscriptions
   ```typescript
   user$ = this.authService.user$.pipe(shareReplay());
   ```

3. **Avoid unnecessary refreshes**: Only call `refreshUserInfo()` when needed

4. **Use `OnPush` detection** for better performance:
   ```typescript
   @Component({
     changeDetection: ChangeDetectionStrategy.OnPush
   })
   ```

---

## Related Files

- Service: `/src/app/services/auth.service.ts`
- Guard: `/src/app/guards/auth.guard.ts`
- Interceptor: `/src/app/interceptors/auth.interceptor.ts`
- Login Component: `/src/app/components/login/`
- Dashboard Component: `/src/app/components/dashboard/`

---

## Support

For issues or questions:
1. Check `FACEBOOK_AUTH_SETUP.md` for setup instructions
2. Review `API_INTEGRATION_GUIDE.md` for API integration
3. See `IMPLEMENTATION_SUMMARY.md` for architecture overview
4. Check browser console for error messages
5. Review Azure Portal logs for auth failures
