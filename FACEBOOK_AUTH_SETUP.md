# Facebook Authentication Setup for Azure Static Web Apps

This guide provides step-by-step instructions to implement Facebook login authentication using Azure Static Web Apps' built-in authentication and the Angular 18 application.

## Architecture Overview

The implementation uses Azure Static Web Apps' built-in authentication provider for Facebook, which provides several key benefits:

- **Secure OAuth 2.0 Flow**: Handled entirely by Azure SWA (no client-side OAuth implementation needed)
- **Session Management**: Automatic cookie-based session handling
- **Protected Routes**: Route-level access control via `staticwebapp.config.json`
- **User Claims**: Access to user information via `/.auth/me` endpoint
- **Server-Side Token Handling**: Credentials never exposed to the browser

## Step 1: Configure Facebook OAuth Application

### 1.1 Create a Facebook App

1. Go to [Facebook Developers](https://developers.facebook.com/)
2. Click "My Apps" > "Create App"
3. Choose "Consumer" as the app type
4. Fill in the app details:
   - **App Name**: Team Manager (or your app name)
   - **App Contact Email**: Your email
   - **Select an App Purpose**: Choose "Other"
5. Click "Create App"

### 1.2 Add Facebook Login Product

1. In your app dashboard, click "Add Product"
2. Find "Facebook Login" and click "Set Up"
3. Choose "Web" as your platform
4. Skip the quick start for now

### 1.3 Configure OAuth Redirect URLs

1. Go to **Settings** > **Basic** to find your:
   - **App ID** (save this as FACEBOOK_APP_ID)
   - **App Secret** (save this as FACEBOOK_APP_SECRET)

2. Go to **Products** > **Facebook Login** > **Settings**
3. Under "Valid OAuth Redirect URIs", add:
   ```
   https://orange-cliff-0c10d4603.azurestaticapps.net/.auth/login/facebook/callback
   ```

   For local development, also add:
   ```
   http://localhost:4200/.auth/login/facebook/callback
   ```

4. Under "Allowed Domains for the JavaScript SDK", add:
   ```
   orange-cliff-0c10d4603.azurestaticapps.net
   localhost:4200
   ```

5. Save changes

### 1.4 Get User Permissions

Go to **Roles** > **Test Users** to create a test user for development/testing.

## Step 2: Configure Azure Static Web Apps

### 2.1 Set Environment Variables

In the Azure Portal:

1. Navigate to your Static Web App resource
2. Go to **Settings** > **Configuration**
3. Add the following application settings:
   - **Name**: `FACEBOOK_APP_ID`
     **Value**: `<your-facebook-app-id>`

   - **Name**: `FACEBOOK_APP_SECRET`
     **Value**: `<your-facebook-app-secret>`

4. Click "Save"

Important: These environment variables are automatically used by `staticwebapp.config.json` for OAuth configuration.

### 2.2 Verify staticwebapp.config.json

Your `staticwebapp.config.json` should contain:

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
      "allowedRoles": ["anonymous"]
    },
    {
      "route": "/dashboard/*",
      "allowedRoles": ["authenticated"]
    },
    {
      "route": "/protected/*",
      "allowedRoles": ["authenticated"]
    },
    {
      "route": "/api/*",
      "allowedRoles": ["anonymous"]
    },
    {
      "route": "/login",
      "allowedRoles": ["anonymous"]
    }
  ]
}
```

## Step 3: Angular Application Structure

### 3.1 Services

**AuthService** (`/src/app/services/auth.service.ts`)
- Manages user authentication state
- Handles login/logout flows
- Provides RxJS observables for reactive components
- Fetches user info from `/.auth/me` endpoint
- Extracts user claims (email, name, etc.)

Key methods:
```typescript
// Get current user info
getUserInfo(): Observable<UserInfo | null>

// Initiate Facebook login
login(): void

// Logout user
logout(): void

// Get user details
getUserEmail(): string | null
getUserName(): string | null
```

### 3.2 Route Guard

**AuthGuard** (`/src/app/guards/auth.guard.ts`)

Functional guard to protect routes from unauthenticated access:

```typescript
export const authGuard: CanActivateFn = (route, state) => {
  // Redirects to /login if user is not authenticated
}
```

### 3.3 HTTP Interceptor

**AuthInterceptor** (`/src/app/interceptors/auth.interceptor.ts`)

Handles:
- 401/403 responses by redirecting to login
- Skips auth endpoints to avoid circular dependencies

### 3.4 Components

**LoginComponent** (`/src/app/components/login/`)
- Public login page
- Facebook login button
- Redirects authenticated users to dashboard

**DashboardComponent** (`/src/app/components/dashboard/`)
- Protected route (requires authentication)
- Displays user info
- Logout functionality

## Step 4: Local Development Setup

### 4.1 Using Azure Static Web Apps CLI

The Azure Static Web Apps CLI allows you to test authentication locally:

```bash
npm install -g @azure/static-web-apps-cli
```

### 4.2 Start Development Server

Create an `swa-cli.config.json` file in your project root:

```json
{
  "configurations": {
    "development": {
      "appLocation": ".",
      "apiLocation": "api",
      "outputLocation": "dist/team-manager",
      "appBuildCommand": "npm run build",
      "apiBuildCommand": "cd api && npm install",
      "run": "npm run start",
      "appDevserverUrl": "http://localhost:4200"
    }
  }
}
```

Start the SWA CLI:

```bash
swa start http://localhost:4200
```

The app will be available at `http://localhost:4200` with the SWA authentication layer.

### 4.3 Test Facebook Login Locally

1. Make sure your Facebook app's redirect URIs include `http://localhost:4200/.auth/login/facebook/callback`
2. Click the "Sign in with Facebook" button
3. You'll be redirected to Facebook's login page
4. After authentication, you'll be redirected back to `/dashboard`

## Step 5: Deployment

### 5.1 Build for Production

```bash
npm run build
```

The build output will be in `dist/team-manager/`.

### 5.2 Deploy to Azure Static Web Apps

Using GitHub Actions (typically automatic on push to main):

The deployment workflow will:
1. Build the Angular app
2. Deploy to Azure Static Web Apps
3. Use the configured FACEBOOK_APP_ID and FACEBOOK_APP_SECRET

### 5.3 Verify Deployment

1. Navigate to your SWA URL: `https://orange-cliff-0c10d4603.azurestaticapps.net`
2. You should see the login page
3. Click "Sign in with Facebook"
4. After successful login, you'll be on the dashboard with your user info displayed

## Security Considerations

### 1. OAuth Token Flow
- Tokens are handled server-side by Azure SWA
- Never exposed to the browser
- Automatically managed with secure cookies

### 2. Environment Variables
- Never commit `FACEBOOK_APP_SECRET` to version control
- Use Azure Portal for configuration
- Use different app credentials for development/production

### 3. CORS Configuration
```json
{
  "route": "/api/*",
  "allowedRoles": ["anonymous"]
}
```
API routes are accessible from your frontend without CORS issues due to same-origin deployment.

### 4. Protected Routes
```json
{
  "route": "/dashboard/*",
  "allowedRoles": ["authenticated"]
}
```
Azure SWA automatically blocks unauthenticated access at the routing level.

## User Information Access

### Available User Claims

After authentication, access user info via `authService.getUserInfo()`:

```json
{
  "identityProvider": "facebook",
  "userId": "123456789",
  "userDetails": "user@example.com",
  "claims": [
    { "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", "val": "user@example.com" },
    { "typ": "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "val": "John Doe" }
  ]
}
```

### Extract Specific Info

```typescript
// In your component
userName = this.authService.getUserName();     // "John Doe"
userEmail = this.authService.getUserEmail();   // "user@example.com"
user$ = this.authService.user$;               // Full user object
```

## Troubleshooting

### Issue: "Invalid OAuth Redirect URI"

**Solution**: Verify that your redirect URL in Facebook Developer Console exactly matches:
```
https://orange-cliff-0c10d4603.azurestaticapps.net/.auth/login/facebook/callback
```

### Issue: "App Not Set Up" Error

**Solution**: Ensure the Facebook Login product is added to your app and the app is in Development or Live mode.

### Issue: 401 Unauthorized on Protected Routes

**Solution**:
1. Check that `staticwebapp.config.json` protects the correct routes
2. Verify the `authGuard` is applied to your routes
3. Check browser cookies for authentication token

### Issue: User Info Not Populated

**Solution**:
1. Ensure `/.auth/me` endpoint is accessible
2. Check browser DevTools Network tab for auth requests
3. Verify user claims are being returned in `AuthService.getUserInfo()`

### Issue: Logout Not Working

**Solution**:
1. Verify logout URL is: `/.auth/logout?post_logout_redirect_uri=<your-url>`
2. Check browser cookies are cleared after logout
3. Test in private browsing mode to rule out cache issues

## Additional Resources

- [Azure Static Web Apps Authentication](https://learn.microsoft.com/en-us/azure/static-web-apps/authentication-authorization)
- [Facebook Login Documentation](https://developers.facebook.com/docs/facebook-login)
- [Angular Router Guards](https://angular.io/guide/router#preventing-unauthorized-access)
- [RxJS Observables](https://rxjs.dev/guide/observable)

## Code Files Reference

All created/modified files:

- `/staticwebapp.config.json` - Azure SWA configuration with auth routes
- `/src/app/services/auth.service.ts` - Authentication service
- `/src/app/guards/auth.guard.ts` - Route protection guard
- `/src/app/interceptors/auth.interceptor.ts` - HTTP error handling
- `/src/app/app.routes.ts` - Application routing
- `/src/app/app.config.ts` - Application configuration with providers
- `/src/app/components/login/` - Login page component
- `/src/app/components/dashboard/` - Protected dashboard component
