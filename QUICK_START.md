# Quick Start Guide - Facebook Authentication

Complete these steps to get Facebook authentication running in 15 minutes.

## Step 1: Facebook Developer Setup (5 minutes)

### 1a. Create Facebook App
1. Go to https://developers.facebook.com/
2. Login or create account
3. Click "My Apps" > "Create App"
4. Select "Consumer" type
5. Fill app details and click "Create App"

### 1b. Add Facebook Login Product
1. In app dashboard, click "Add Product"
2. Find "Facebook Login" and click "Set Up"
3. Choose "Web" platform
4. Click "Settings" to open Facebook Login settings

### 1c. Configure Redirect URIs
Add these two URLs under "Valid OAuth Redirect URIs":

**Production:**
```
https://orange-cliff-0c10d4603.azurestaticapps.net/.auth/login/facebook/callback
```

**Development:**
```
http://localhost:4200/.auth/login/facebook/callback
```

### 1d. Get Your Credentials
1. Go to **Settings > Basic**
2. Copy and save:
   - **App ID** → Save as FACEBOOK_APP_ID
   - **App Secret** → Save as FACEBOOK_APP_SECRET

## Step 2: Configure Azure Static Web Apps (3 minutes)

### 2a. Set Environment Variables
1. Go to [Azure Portal](https://portal.azure.com)
2. Find your Static Web App resource
3. Click **Settings > Configuration**
4. Add two new application settings:

| Setting Name | Value |
|---|---|
| FACEBOOK_APP_ID | (paste your App ID) |
| FACEBOOK_APP_SECRET | (paste your App Secret) |

5. Click **Save**

### 2b. Redeploy
Push to GitHub to trigger a new deployment:
```bash
git add .
git commit -m "Configure Facebook authentication"
git push
```

## Step 3: Build and Deploy (2 minutes)

### 3a. Build Angular
```bash
npm run build
```

### 3b. Test Locally (Optional)
```bash
npm install -g @azure/static-web-apps-cli
swa start http://localhost:4200
```

Then visit http://localhost:4200 to test the login flow.

### 3c. Deploy
Push your changes to main branch. GitHub Actions will automatically deploy.

## Step 4: Test the Live App (5 minutes)

1. Open https://orange-cliff-0c10d4603.azurestaticapps.net
2. Click "Sign in with Facebook"
3. Login with Facebook test user
4. See dashboard with your user info
5. Click "Logout" to test logout

## Done!

Your app now has Facebook authentication configured and deployed.

## What Was Already Set Up For You

All the Angular code has been implemented:

- **AuthService** - Handles login/logout and user info
- **authGuard** - Protects routes that need authentication
- **LoginComponent** - Beautiful login page
- **DashboardComponent** - Protected dashboard showing user info
- **HTTP Interceptor** - Handles authentication errors
- **Route Configuration** - Sets up protected routes
- **staticwebapp.config.json** - Configures Azure SWA authentication

## File Reference

| File | Purpose |
|---|---|
| `/staticwebapp.config.json` | Azure auth configuration |
| `/src/app/services/auth.service.ts` | Authentication logic |
| `/src/app/components/login/` | Login page |
| `/src/app/components/dashboard/` | Protected dashboard |

## Common Issues & Fixes

### Issue: "Invalid OAuth Redirect URI"
**Fix**: Make sure your redirect URL in Facebook app settings EXACTLY matches:
```
https://orange-cliff-0c10d4603.azurestaticapps.net/.auth/login/facebook/callback
```

### Issue: Login button doesn't work
**Fix**:
1. Check that FACEBOOK_APP_ID and FACEBOOK_APP_SECRET are set in Azure Portal
2. Verify the Static Web App has been redeployed after setting credentials
3. Check browser console for errors

### Issue: User info not showing on dashboard
**Fix**:
1. Check that you're logged in (look for authentication cookie in DevTools)
2. Verify the /.auth/me endpoint returns user info
3. Check browser console for any JavaScript errors

### Issue: Logout doesn't work
**Fix**:
1. Try in a private browsing window
2. Check that browser accepts cookies
3. Clear site data and try again

## Next Steps

1. **Customize Login Page**: Edit `/src/app/components/login/login.component.html`
2. **Customize Dashboard**: Edit `/src/app/components/dashboard/dashboard.component.html`
3. **Add More Routes**: Add new protected routes in `/src/app/app.routes.ts`
4. **Connect to APIs**: See `API_INTEGRATION_GUIDE.md` for calling your backend

## For More Help

- **Setup Instructions**: See `FACEBOOK_AUTH_SETUP.md`
- **API Integration**: See `API_INTEGRATION_GUIDE.md`
- **Full Implementation Details**: See `IMPLEMENTATION_SUMMARY.md`

## Key URLs

- **Your App**: https://orange-cliff-0c10d4603.azurestaticapps.net
- **Login Page**: https://orange-cliff-0c10d4603.azurestaticapps.net/login
- **Dashboard**: https://orange-cliff-0c10d4603.azurestaticapps.net/dashboard
- **Auth Info**: https://orange-cliff-0c10d4603.azurestaticapps.net/.auth/me (when logged in)

## Verify Installation

Check that these files exist in your project:

```
src/app/
  ├── services/auth.service.ts          ✓
  ├── guards/auth.guard.ts               ✓
  ├── interceptors/auth.interceptor.ts   ✓
  ├── components/login/                  ✓
  ├── components/dashboard/              ✓
  ├── app.routes.ts                      ✓
  └── app.config.ts                      ✓

Root files:
  ├── staticwebapp.config.json            ✓
  ├── FACEBOOK_AUTH_SETUP.md              ✓
  ├── API_INTEGRATION_GUIDE.md            ✓
  ├── IMPLEMENTATION_SUMMARY.md           ✓
  └── QUICK_START.md                      ✓
```

All files have been created. You're ready to go!
