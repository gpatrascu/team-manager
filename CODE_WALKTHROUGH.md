# Code Walkthrough - Facebook Authentication Implementation

This document explains the key code components and how they work together.

## Authentication Service

**File**: `/src/app/services/auth.service.ts`

The heart of the authentication system. This service:
- Manages user state via RxJS BehaviorSubjects
- Fetches user info from Azure SWA's `/.auth/me` endpoint
- Handles login/logout redirects
- Extracts user claims (email, name, etc.)

### Key Methods

```typescript
// Constructor - automatically initializes authentication
constructor(private http: HttpClient) {
  this.initializeAuth();  // Fetch current user on app startup
}

// Public observables components can subscribe to
public user$: Observable<UserInfo | null>;  // Current user
public isAuthenticated$: Observable<boolean>;  // Auth status
public isLoading$: Observable<boolean>;  // Loading state

// Get current user synchronously
public getCurrentUser(): UserInfo | null

// Redirect to Facebook OAuth
public login(): void {
  window.location.href = '/.auth/login/facebook?post_login_redirect_uri=/dashboard';
}

// Clear session and logout
public logout(): void {
  window.location.href = '/.auth/logout?post_logout_redirect_uri=/';
}

// Extract email from user claims
public getUserEmail(): string | null

// Extract name from user claims
public getUserName(): string | null

// Refresh user info from server
public refreshUserInfo(): Observable<UserInfo | null>
```

## Route Guard

**File**: `/src/app/guards/auth.guard.ts`

Protects routes that require authentication. Applied to routes like `/dashboard`.

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If already authenticated, allow access
  if (authService.isAuthenticated()) {
    return true;
  }

  // If not authenticated, redirect to /login
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};

// Usage in routes:
{
  path: 'dashboard',
  component: DashboardComponent,
  canActivate: [authGuard]  // This route is protected
}
```

## HTTP Interceptor

**File**: `/src/app/interceptors/auth.interceptor.ts`

Handles authentication errors on API calls.

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  // Skip interceptor for auth endpoints
  const isAuthEndpoint = req.url.includes('/.auth/');
  if (isAuthEndpoint) {
    return next(req);
  }

  // Process response and catch errors
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // If 401 or 403, user not authenticated
      if (error.status === 401 || error.status === 403) {
        router.navigate(['/login'], {
          queryParams: { returnUrl: router.url }
        });
      }
      return throwError(() => error);
    })
  );
};
```

## Login Component

**File**: `/src/app/components/login/login.component.ts`

Simple component that initiates login.

```typescript
export class LoginComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // If already authenticated, go to dashboard
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  loginWithFacebook(): void {
    // This calls authService.login() which redirects to Facebook
    this.authService.login();
  }
}
```

## Dashboard Component

**File**: `/src/app/components/dashboard/dashboard.component.ts`

Protected component that displays user info.

```typescript
export class DashboardComponent implements OnInit {
  // Observable of current user
  user$: Observable<UserInfo | null>;

  // Sync user data
  userName: string | null = null;
  userEmail: string | null = null;

  constructor(private authService: AuthService) {
    this.user$ = this.authService.user$;
  }

  ngOnInit(): void {
    // Get user info synchronously
    const user = this.authService.getCurrentUser();
    if (user) {
      this.userName = this.authService.getUserName();
      this.userEmail = this.authService.getUserEmail();
    }
  }

  logout(): void {
    // This redirects to logout and clears session
    this.authService.logout();
  }
}
```

## Route Configuration

**File**: `/src/app/app.routes.ts`

Defines the app's routes and applies guards.

```typescript
export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginComponent
    // No guard - publicly accessible
  },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [authGuard]  // Protected by guard
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
```

## App Configuration

**File**: `/src/app/app.config.ts`

Sets up Angular providers and HTTP interceptor.

```typescript
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),  // Enable routing
    provideHttpClient(
      withInterceptors([authInterceptor])  // Enable auth interceptor
    )
  ]
};
```

## Azure SWA Configuration

**File**: `/staticwebapp.config.json`

Configures Azure Static Web Apps authentication and routing.

```json
{
  "auth": {
    "identityProviders": {
      "facebook": {
        "registration": {
          "appIdSettingName": "FACEBOOK_APP_ID",
          "appSecretSettingName": "FACEBOOK_APP_SECRET"
        }
      }
    }
  },
  "routes": [
    {
      "route": "/.auth/*",
      "allowedRoles": ["anonymous"]  // Auth endpoints are public
    },
    {
      "route": "/dashboard/*",
      "allowedRoles": ["authenticated"]  // Protected at SWA level
    },
    {
      "route": "/login",
      "allowedRoles": ["anonymous"]  // Login page is public
    }
  ]
}
```

## App Component

**File**: `/src/app/app.component.ts`

The root component that handles loading state and routing outlet.

```typescript
export class AppComponent implements OnInit {
  isLoading$ = this.authService.isLoading$;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    // AuthService automatically initializes authentication
  }
}
```

**Template** (`app.component.html`):
```html
<div class="app-container" *ngIf="!(isLoading$ | async); else loading">
  <router-outlet></router-outlet>  <!-- Routes render here -->
</div>

<ng-template #loading>
  <div class="loading-screen">
    <div class="loading-spinner"></div>
    <p>Loading...</p>
  </div>
</ng-template>
```

## Data Flow Diagram

### Startup Flow

```
1. App bootstrap
   ↓
2. AppComponent loads
   ↓
3. AuthService.constructor() called
   ↓
4. initializeAuth() starts
   ↓
5. HTTP GET /.auth/me sent
   ↓
6. Azure SWA responds with user or null
   ↓
7. BehaviorSubjects updated:
   - user$ = user or null
   - isAuthenticated$ = boolean
   - isLoading$ = false
   ↓
8. Components subscribe to observables
   ↓
9. UI renders based on auth status
```

### Login Flow

```
1. User clicks "Sign in with Facebook"
   ↓
2. LoginComponent.loginWithFacebook() called
   ↓
3. AuthService.login() called
   ↓
4. window.location.href = /.auth/login/facebook?...
   ↓
5. Azure SWA receives request
   ↓
6. Azure SWA redirects to Facebook OAuth
   ↓
7. User authenticates with Facebook
   ↓
8. Facebook redirects to callback URL
   ↓
9. Azure SWA validates token
   ↓
10. Session cookie created
    ↓
11. Redirect to /dashboard
    ↓
12. AppComponent checks auth on load
    ↓
13. authGuard allows access
    ↓
14. DashboardComponent renders
    ↓
15. DashboardComponent requests /.auth/me
    ↓
16. User info displays
```

### Protected Route Access Flow

```
1. User tries to access /dashboard
   ↓
2. authGuard checks isAuthenticated()
   ├─ YES: Allow component to load
   └─ NO: Redirect to /login
```

### API Call Flow

```
1. Component calls this.http.get('/api/teams')
   ↓
2. Request reaches authInterceptor
   ↓
3. Interceptor skips /.auth/* check
   ↓
4. Request passes through
   ↓
5. Session cookie automatically sent
   ↓
6. Backend receives request with user info
   ↓
7. Backend validates user claims
   ↓
8. Backend returns response
   ↓
9. Response received by component
   ├─ 200: Data displayed
   ├─ 401/403: authInterceptor redirects to /login
   └─ Other: Error handled by component
```

## Type Definitions

### UserInfo Interface

```typescript
interface UserInfo {
  identityProvider: string;  // "facebook"
  userId: string;            // User ID from Facebook
  userDetails: string;       // Email or name
  claims: Array<{
    typ: string;            // Claim type (email, name, etc.)
    val: string;            // Claim value
  }>;
}
```

## How Azure SWA's Built-In Auth Works

```
1. Request comes to Azure SWA
   ↓
2. SWA checks if session cookie exists
   ├─ No valid cookie: Returns 401
   └─ Valid cookie: Decrypts and validates
   ↓
3. If valid:
   - Sets HttpContext.User with claims
   - Allows request through
   - Request reaches Angular app or backend
   ↓
4. /.auth/me endpoint returns user info
   - clientPrincipal object from validated cookie
   - Claims from Facebook user data
   - All user data comes from cookie (no secrets exposed)
```

## Security Boundaries

```
Layer 1: Azure SWA Route Protection
  Blocks requests to authenticated routes if no valid cookie
  Returns 401 Unauthorized

Layer 2: Angular Route Guard
  Prevents component creation in browser
  Redirects to /login

Layer 3: HTTP Interceptor
  Catches API errors
  Handles 401/403 responses

Layer 4: Backend Authorization
  Validates claims on server
  Enforces data access policies
```

## Key Concepts

### Observable vs Promise

The service uses RxJS Observables instead of Promises:

```typescript
// Observable (used in this service)
user$: Observable<UserInfo | null>

// In component
this.user$ = this.authService.user$;
// Or
this.user$.subscribe(user => console.log(user));
// Or in template
<div *ngIf="user$ | async as user">{{ user.userDetails }}</div>
```

### BehaviorSubject

The service uses BehaviorSubjects internally:

```typescript
// In AuthService
private userSubject = new BehaviorSubject<UserInfo | null>(null);

// When data changes
userSubject.next(newUser);  // All subscribers notified

// Export as observable
public user$ = this.userSubject.asObservable();
```

### Angular Router Guards

The authGuard is a functional guard:

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  // return true to allow navigation
  // return false to block navigation
  // return Observable<boolean> for async checks
}
```

### HTTP Interceptors

The authInterceptor processes all HTTP requests:

```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Modify request if needed
  // Process response
  return next(req).pipe(
    tap(response => { /* success */ }),
    catchError(error => { /* handle error */ })
  );
}
```

## Common Component Patterns

### Pattern 1: Display User Name

```typescript
userName = this.authService.getUserName();

// In template
<p>Welcome, {{ userName }}</p>
```

### Pattern 2: Show Different UI Based on Auth

```typescript
constructor(private authService: AuthService) {}

ngOnInit() {
  this.authService.isAuthenticated$.subscribe(isAuth => {
    if (isAuth) {
      // Show authenticated UI
    } else {
      // Show login prompt
    }
  });
}
```

### Pattern 3: Make Protected API Call

```typescript
constructor(private http: HttpClient) {}

getTeams() {
  return this.http.get('/api/teams');
  // Session cookie automatically included
  // 401 errors handled by authInterceptor
}
```

### Pattern 4: Logout

```typescript
logout() {
  this.authService.logout();
  // Clears session and redirects to home
}
```

## Testing Patterns

### Mock AuthService

```typescript
const mockAuthService = {
  user$: of(null),
  isAuthenticated$: of(false),
  isLoading$: of(false),
  getCurrentUser: () => null,
  getUserName: () => null,
  getUserEmail: () => null,
  login: jasmine.createSpy(),
  logout: jasmine.createSpy()
};

// In test
TestBed.configureTestingModule({
  providers: [
    { provide: AuthService, useValue: mockAuthService }
  ]
});
```

## Performance Considerations

1. **Lazy Loading**: Routes can be lazy-loaded if the app grows
2. **Change Detection**: Use OnPush strategy for better performance
3. **Unsubscribe**: Use async pipe to avoid manual subscriptions
4. **Caching**: User info is cached in BehaviorSubject
5. **Cold Start**: /.auth/me is only called on app startup

## Common Modifications

### Change Login Redirect URL

In AuthService:
```typescript
public login(): void {
  const redirectUrl = this.LOGIN_ENDPOINT + '?post_login_redirect_uri='
    + encodeURIComponent(window.location.origin + '/dashboard');
  // Change '/dashboard' to desired URL
}
```

### Add Custom Claims Extraction

In AuthService:
```typescript
public getUserPhone(): string | null {
  const user = this.userSubject.value;
  if (!user) return null;
  const claim = user.claims?.find(c => c.typ === 'phone_claim_type');
  return claim?.val || null;
}
```

### Add Role-Based Access

Create new guard:
```typescript
export const adminGuard: CanActivateFn = (route, state) => {
  const roleService = inject(RoleService);
  return roleService.isAdmin();
};
```

Apply to routes:
```typescript
{
  path: 'admin',
  component: AdminComponent,
  canActivate: [authGuard, adminGuard]
}
```

## Debugging Tips

1. **Check Auth Status**:
   ```typescript
   console.log(this.authService.isAuthenticated());
   ```

2. **Check User Data**:
   ```typescript
   console.log(this.authService.getCurrentUser());
   ```

3. **Check Observable**:
   ```typescript
   this.authService.user$.subscribe(user => console.log(user));
   ```

4. **Check Cookies**:
   - Open DevTools > Application > Cookies
   - Look for authentication cookie with SameSite=None

5. **Check API Response**:
   - Open DevTools > Network
   - Look for /.auth/me request
   - Check response status and data

6. **Check Errors**:
   - Open DevTools > Console
   - Look for any JavaScript errors
   - Check for network errors in Network tab

## Summary

The authentication system works by:
1. **AuthService** manages user state and communicates with Azure SWA
2. **Route guards** prevent unauthorized navigation
3. **HTTP interceptor** handles authentication errors
4. **Components** display UI based on authentication status
5. **Azure SWA** provides the actual OAuth integration
6. **Session cookies** maintain user sessions securely

Everything works together to create a secure, reactive authentication system with minimal complexity.
