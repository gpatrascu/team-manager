# Facebook Authentication Architecture

Visual guide to understanding the complete authentication system architecture.

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    User's Browser / Angular App                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │              Angular Application (Angular 18)            │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │                                                            │   │
│  │  ┌────────────────────────────────────────────────────┐  │   │
│  │  │              AppComponent (Root)                   │  │   │
│  │  │  - Shows loading screen during auth check          │  │   │
│  │  │  - Routes to login or dashboard                    │  │   │
│  │  └────────────────────────────────────────────────────┘  │   │
│  │                          │                                 │   │
│  │        ┌─────────────────┴──────────────────┐             │   │
│  │        │                                    │             │   │
│  │  ┌─────▼──────────┐         ┌────────────────▼──┐        │   │
│  │  │ LoginComponent │         │ DashboardComponent │       │   │
│  │  ├────────────────┤         ├───────────────────┤        │   │
│  │  │ - Login button │         │ - User info       │        │   │
│  │  │ - Calls login()│         │ - Logout button   │        │   │
│  │  │ - Beautiful UI │         │ - Protected route │        │   │
│  │  └────────────────┘         └───────────────────┘        │   │
│  │                                                            │   │
│  │  ┌────────────────────────────────────────────────────┐  │   │
│  │  │          AuthService (Singleton)                   │  │   │
│  │  ├────────────────────────────────────────────────────┤  │   │
│  │  │ Observables:                                        │  │   │
│  │  │  - user$: Current user info                         │  │   │
│  │  │  - isAuthenticated$: Auth status                    │  │   │
│  │  │  - isLoading$: Loading state                        │  │   │
│  │  │                                                      │  │   │
│  │  │ Methods:                                             │  │   │
│  │  │  - login(): Redirect to OAuth                       │  │   │
│  │  │  - logout(): Clear session                          │  │   │
│  │  │  - getUserInfo(): Fetch from /.auth/me             │  │   │
│  │  │  - getUserEmail/Name(): Extract from claims        │  │   │
│  │  │  - getCurrentUser(): Sync access                    │  │   │
│  │  └────────────────────────────────────────────────────┘  │   │
│  │                          │                                 │   │
│  │  ┌───────────────────────┴──────────────────────────┐    │   │
│  │  │                                                    │    │   │
│  │  ▼                                                    ▼    │   │
│  │ ┌──────────────┐                            ┌──────────┐ │   │
│  │ │ authGuard    │                            │authInter-│ │   │
│  │ ├──────────────┤                            │ceptor    │ │   │
│  │ │- Protect     │                            ├──────────┤ │   │
│  │ │  routes      │                            │- Handle  │ │   │
│  │ │- Redirect to │                            │  401/403 │ │   │
│  │ │  /login      │                            │- Skip    │ │   │
│  │ │- Check auth  │                            │  /.auth/*│ │   │
│  │ │  status      │                            └──────────┘ │   │
│  │ └──────────────┘                                          │   │
│  │                                                            │   │
│  │  ┌────────────────────────────────────────────────────┐  │   │
│  │  │         app.routes.ts (Route Config)               │  │   │
│  │  ├────────────────────────────────────────────────────┤  │   │
│  │  │ /login (public, no guard)                          │  │   │
│  │  │ /dashboard (protected, authGuard)                  │  │   │
│  │  │ /* -> /dashboard (default redirect)                │  │   │
│  │  └────────────────────────────────────────────────────┘  │   │
│  │                                                            │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
└──────────────────────────────────────────┬──────────────────────┘
                                           │
                     ┌─────────────────────┴──────────────────┐
                     │                                        │
                     │ HTTP Requests                          │
                     │ (With Session Cookie)                  │
                     │                                        │
                     ▼                                        ▼
┌──────────────────────────────────────────────────────────────────┐
│        Azure Static Web Apps (blue-cliff-0c10d4603)              │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │           staticwebapp.config.json Config               │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │ Auth Provider: Facebook                                  │   │
│  │   - App ID: FACEBOOK_APP_ID (env var)                   │   │
│  │   - Secret: FACEBOOK_APP_SECRET (env var)               │   │
│  │                                                          │   │
│  │ Protected Routes:                                        │   │
│  │   - /dashboard/* (authenticated only)                   │   │
│  │   - /protected/* (authenticated only)                   │   │
│  │                                                          │   │
│  │ Public Routes:                                           │   │
│  │   - /login (anonymous)                                  │   │
│  │   - /api/* (anonymous)                                  │   │
│  │   - /.auth/* (anonymous)                                │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │        Built-In Authentication Handler                   │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │                                                          │   │
│  │  GET /.auth/me                                          │   │
│  │  └─> Returns: { clientPrincipal: UserInfo }            │   │
│  │      (Requires valid session cookie)                    │   │
│  │                                                          │   │
│  │  GET /.auth/login/facebook                             │   │
│  │  └─> Redirects to Facebook OAuth endpoint              │   │
│  │      post_login_redirect_uri = /dashboard              │   │
│  │                                                          │   │
│  │  GET /.auth/login/facebook/callback                    │   │
│  │  └─> OAuth callback from Facebook                      │   │
│  │      Creates session cookie                            │   │
│  │      Redirects to post_login_redirect_uri             │   │
│  │                                                          │   │
│  │  GET /.auth/logout                                     │   │
│  │  └─> Clears session cookie                             │   │
│  │      post_logout_redirect_uri = /                       │   │
│  │                                                          │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │         API Routes (Optional Backend)                    │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │ /api/* routes can access:                               │   │
│  │   - HttpContext.User (authenticated user claims)        │   │
│  │   - Session data via Azure functions                    │   │
│  │   - Database connections                                │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
└──────────────────────────────────────────┬──────────────────────┘
                                           │
                                    (Facebook OAuth)
                                           │
                     ┌─────────────────────┴──────────────────┐
                     │                                        │
                     ▼                                        ▼
┌──────────────────────────────────────┐    ┌────────────────────┐
│   Facebook OAuth Server              │    │  Facebook User     │
│                                      │    │  Database          │
│ /oauth/authorize                     │    │                    │
│ /oauth/access_token                  │    │ Stores user info:  │
│                                      │    │ - Name             │
│ Validates:                           │    │ - Email            │
│ - Client ID                          │    │ - Profile picture  │
│ - Client Secret                      │    │ - etc.             │
│ - Redirect URI                       │    │                    │
│ - User credentials                   │    │                    │
└──────────────────────────────────────┘    └────────────────────┘
```

## Data Flow Diagram

### Initial Authentication Check (App Startup)

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. AppComponent Initializes                                     │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. AuthService.constructor() called                             │
│    - Sets isLoading$ = true                                     │
│    - Calls initializeAuth()                                     │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. HTTP GET /.auth/me (via AuthService)                        │
│    - Sent to Azure SWA                                          │
│    - Includes session cookie (if exists)                        │
└─────────────────────────────────────────────────────────────────┘
                           │
         ┌─────────────────┴─────────────────┐
         │                                   │
         ▼                                   ▼
    ┌─────────────┐               ┌──────────────────┐
    │ Has Cookie  │               │ No Cookie        │
    │ & Valid     │               │ (Not Logged In)  │
    └─────────────┘               └──────────────────┘
         │                               │
         ▼                               ▼
    ┌────────────────┐         ┌──────────────────┐
    │ Returns User   │         │ Returns 401 or   │
    │ Info Object    │         │ clientPrincipal: │
    │ with claims    │         │ null             │
    └────────────────┘         └──────────────────┘
         │                               │
         ▼                               ▼
    ┌──────────────────────────────────────────┐
    │ AuthService updates:                     │
    │ - user$ = UserInfo                       │
    │ - isAuthenticated$ = true                │
    │ - isLoading$ = false                     │
    └──────────────────────────────────────────┘
         │                               │
         ▼                               ▼
    ┌──────────────────┐     ┌──────────────────┐
    │ authGuard allows │     │ authGuard        │
    │ access to routes │     │ redirects to     │
    │ marked as        │     │ /login           │
    │ authenticated    │     │                  │
    └──────────────────┘     └──────────────────┘
```

### Login Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. User clicks "Sign in with Facebook" button                   │
│    LoginComponent.loginWithFacebook() called                    │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. AuthService.login() called                                   │
│    - Constructs URL:                                            │
│      /.auth/login/facebook?post_login_redirect_uri=/dashboard   │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. Browser redirects to Azure SWA OAuth endpoint                │
│    window.location.href = /.auth/login/facebook?...             │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 4. Azure SWA initiates Facebook OAuth flow                      │
│    - Reads FACEBOOK_APP_ID and FACEBOOK_APP_SECRET             │
│    - Redirects to Facebook login page                           │
│    - Includes redirect_uri: /.auth/login/facebook/callback     │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 5. User logs in with Facebook credentials                       │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 6. Facebook redirects back to callback URL                      │
│    /.auth/login/facebook/callback?code=...&state=...           │
│    Includes authorization code                                  │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 7. Azure SWA receives callback                                  │
│    - Exchanges code for access token with Facebook             │
│    - Fetches user info from Facebook                           │
│    - Creates session cookie                                     │
│    - Redirects to post_login_redirect_uri (/dashboard)         │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 8. Browser receives redirect and navigates to /dashboard        │
│    Session cookie is set by browser                             │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 9. Angular app loads /dashboard                                 │
│    authGuard allows access (session cookie present)             │
│    DashboardComponent loads                                     │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 10. DashboardComponent requests /.auth/me                       │
│     - Session cookie automatically sent                         │
│     - Azure SWA returns user info                               │
│     - Component displays user data                              │
└─────────────────────────────────────────────────────────────────┘
```

### Logout Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. User clicks "Logout" button                                  │
│    DashboardComponent.logout() called                           │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. AuthService.logout() called                                  │
│    - Constructs URL:                                            │
│      /.auth/logout?post_logout_redirect_uri=/                   │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. Browser redirects to Azure SWA logout endpoint               │
│    window.location.href = /.auth/logout?...                    │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 4. Azure SWA clears session                                     │
│    - Deletes session cookie                                     │
│    - Redirects to post_logout_redirect_uri (/)                  │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 5. Browser receives redirect and navigates to /                 │
│    Session cookie is deleted                                    │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 6. Angular app redirects to /dashboard                          │
│    authGuard checks auth status                                 │
│    - No session cookie found                                    │
│    - isAuthenticated$ = false                                   │
│    - Redirects to /login                                        │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 7. LoginComponent loads                                          │
│    User is back to login page                                   │
└─────────────────────────────────────────────────────────────────┘
```

### Protected API Call

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. Component calls protected API                                │
│    this.http.get('/api/teams')                                  │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. HTTP Request Interceptor processes request                   │
│    - Skips /.auth/* endpoints                                   │
│    - Allows request to continue                                 │
│    - Session cookie automatically included by browser           │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. Request reaches Azure SWA routing layer                      │
│    - URL: /api/teams                                            │
│    - Config: allowedRoles: ["anonymous"]                        │
│    - Allows request through (or checks role if needed)         │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 4. Request routed to backend API (Azure Functions/App Service)  │
│    - Session cookie available in HttpContext                    │
│    - User claims available in HttpContext.User                  │
│    - Backend extracts userId from claims                        │
│    - Fetches data for that user from database                   │
└─────────────────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────────────────┐
│ 5. Backend returns response                                     │
│    - Status 200 with user's teams data                          │
│    - Or 401 if session invalid                                  │
└─────────────────────────────────────────────────────────────────┘
                           │
         ┌─────────────────┴─────────────────┐
         │                                   │
         ▼                                   ▼
    ┌─────────────┐              ┌──────────────────┐
    │ Success 200 │              │ Unauthorized 401 │
    └─────────────┘              └──────────────────┘
         │                               │
         ▼                               ▼
    ┌──────────────────────┐    ┌────────────────────┐
    │ Response interceptor  │    │ authInterceptor    │
    │ processes data        │    │ catches 401        │
    │ Component receives    │    │ Redirects to       │
    │ teams array           │    │ /login             │
    │ Updates UI            │    │ Clears auth state  │
    └──────────────────────┘    └────────────────────┘
```

## Component Hierarchy

```
AppComponent
├── isLoading$ loading state
├── Routes outlet
│
├── LoginComponent (route: /login)
│   ├── Uses AuthService.login()
│   ├── Displays Facebook login button
│   └── Redirects authenticated users to dashboard
│
└── DashboardComponent (route: /dashboard, canActivate: [authGuard])
    ├── Protected by authGuard
    ├── Uses AuthService to get user info
    ├── Displays user.userDetails
    ├── Shows user.claims
    └── Logout button calls AuthService.logout()
```

## Observable Flow

```
AuthService Internal State
┌────────────────────────────────────────────┐
│                                            │
│  Private BehaviorSubjects:                 │
│  ├─ userSubject: BehaviorSubject           │
│  ├─ isAuthenticatedSubject: BehaviorSubject│
│  └─ isLoadingSubject: BehaviorSubject      │
│                                            │
│  Public Observables (expose subjects):     │
│  ├─ user$: Observable<UserInfo | null>    │
│  ├─ isAuthenticated$: Observable<bool>    │
│  └─ isLoading$: Observable<bool>          │
│                                            │
│  Changes flow:                             │
│  ┌──────────────────────────────────────┐  │
│  │ initializeAuth()                     │  │
│  │ ↓                                    │  │
│  │ getUserInfo() [HTTP call]            │  │
│  │ ↓                                    │  │
│  │ Response received                    │  │
│  │ ↓                                    │  │
│  │ userSubject.next(user)               │  │
│  │ isAuthenticatedSubject.next(bool)    │  │
│  │ isLoadingSubject.next(false)         │  │
│  │ ↓                                    │  │
│  │ All observables emit new values      │  │
│  │ ↓                                    │  │
│  │ Components (async pipe) update UI    │  │
│  └──────────────────────────────────────┘  │
│                                            │
└────────────────────────────────────────────┘
```

## Security Boundaries

```
┌─────────────────────────────────────────────────────────────────┐
│                     Application Security                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Level 1: Azure SWA Route Protection                      │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │ - Routes marked "authenticated" block unauthenticated    │   │
│  │ - Returns 401 if no valid session cookie                │   │
│  │ - No request reaches backend if not authenticated        │   │
│  └──────────────────────────────────────────────────────────┘   │
│                           │ Authenticated requests pass through   │
│                           │                                       │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Level 2: Angular Route Guard (authGuard)                │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │ - Checks AuthService.isAuthenticated()                  │   │
│  │ - Redirects unauthenticated users to /login            │   │
│  │ - Prevents component creation if not authenticated       │   │
│  └──────────────────────────────────────────────────────────┘   │
│                           │ Authenticated Angular routes pass    │
│                           │                                      │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Level 3: HTTP Interceptor (authInterceptor)             │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │ - Catches 401/403 responses                             │   │
│  │ - Redirects to login if API returns unauthorized        │   │
│  │ - Provides additional error handling                     │   │
│  └──────────────────────────────────────────────────────────┘   │
│                           │ API requests with valid cookies pass  │
│                           │                                       │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ Level 4: Backend API Authorization                       │   │
│  ├──────────────────────────────────────────────────────────┤   │
│  │ - Backend validates user claims                         │   │
│  │ - Ensures user can only access their own data           │   │
│  │ - Returns 403 Forbidden for unauthorized operations      │   │
│  └──────────────────────────────────────────────────────────┘   │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘

Multiple layers ensure defense-in-depth security approach
```

## Session Cookie Lifecycle

```
┌──────────────────────────────────────────────────────────┐
│ Login Success                                            │
└──────────────────────────────────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────┐
│ Azure SWA creates session cookie:                        │
│ - Name: [system-generated]                              │
│ - Value: [encrypted session token]                      │
│ - HttpOnly: true (not accessible to JavaScript)         │
│ - Secure: true (only sent over HTTPS)                   │
│ - SameSite: None (for cross-origin scenarios)           │
│ - Expires: After configured TTL (typically 24-72 hours) │
└──────────────────────────────────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────┐
│ Browser stores cookie and includes in all requests       │
│ to https://orange-cliff-0c10d4603.azurestaticapps.net/  │
└──────────────────────────────────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────┐
│ Azure SWA validates cookie on each request:              │
│ - Decrypts session token                                 │
│ - Verifies authenticity and expiration                   │
│ - Sets HttpContext.User with user claims                │
│ - Allows request through authenticated routes            │
└──────────────────────────────────────────────────────────┘
           │
      ┌────┴────┐
      │          │
      ▼          ▼
  ┌────────┐  ┌────────────┐
  │ Logout │  │ Expiration │
  └────────┘  └────────────┘
      │          │
      └────┬─────┘
           ▼
┌──────────────────────────────────────────────────────────┐
│ Cookie Deleted:                                          │
│ - Azure SWA clears cookie on logout                      │
│ - Browser automatically deletes on expiration            │
│ - Next request has no valid session                      │
└──────────────────────────────────────────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────┐
│ 401 Unauthorized Response                                │
│ User must login again                                    │
└──────────────────────────────────────────────────────────┘
```

## Environment Variables Usage

```
Azure Portal Configuration
    │
    ├─ FACEBOOK_APP_ID
    │  └─> Loaded by Azure SWA
    │      └─> Used in OAuth flow
    │          └─> Passed to Facebook
    │
    └─ FACEBOOK_APP_SECRET
       └─> Loaded by Azure SWA (server-side only)
           └─> Used to sign OAuth tokens
               └─> Never exposed to browser

staticwebapp.config.json references:
{
  "auth": {
    "identityProviders": {
      "facebook": {
        "registration": {
          "appIdSettingName": "FACEBOOK_APP_ID",          ← Points to env var
          "appSecretSettingName": "FACEBOOK_APP_SECRET"   ← Points to env var
        }
      }
    }
  }
}

This design ensures:
- Credentials are stored securely in Azure
- Never committed to version control
- Not exposed to frontend JavaScript
- Automatically available to Azure SWA auth handler
```

## Technology Stack Summary

```
Frontend Layer (Browser)
├── Angular 18 (Framework)
├── TypeScript (Language)
├── RxJS (Reactive programming)
└── CSS (Styling)

Application Layer (Node.js)
├── Angular Router (Routing)
├── HttpClient (HTTP requests)
├── Services (Business logic)
└── Guards & Interceptors (Authentication)

Azure SWA Layer
├── Authentication Provider (Facebook OAuth)
├── Route Configuration
├── Session Management
└── HTTPS/TLS (Security)

External Services
├── Facebook OAuth (Authentication)
└── Optional: Azure Functions or App Service (API backend)
```

This architecture ensures:
1. Clean separation of concerns
2. Multiple security layers
3. Scalability for future features
4. Type safety throughout
5. Production-ready implementation
