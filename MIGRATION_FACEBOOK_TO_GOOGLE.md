# Migration: Facebook to Google Authentication

## Overview

This document summarizes the migration from Facebook OAuth authentication to Google OAuth authentication for the Team Manager application on Azure Static Web Apps.

**Migration Date:** December 26, 2025
**Reason:** Simplify OAuth provider setup - Google is significantly easier to configure than Facebook

---

## Why Google Over Facebook?

### Google Advantages
- **Simpler Setup:** 5-10 minutes vs 30-60 minutes for Facebook
- **Single OAuth Console:** All settings in one place
- **No App Verification:** Immediate credentials without waiting for approval
- **More Reliable:** Stable authentication with fewer verification issues
- **Better Documentation:** Clearer setup instructions

### Setup Comparison

| Factor | Facebook | Google |
|--------|----------|--------|
| Project Creation | Requires Meta business account | Google account (free) |
| OAuth Approval | 5-7 business days | Immediate |
| Credential Types | Multiple app types | Single OAuth app |
| Configuration Locations | 5+ different pages | Single credentials page |
| Learning Curve | Steep | Gentle |

---

## Files Modified

### 1. staticwebapp.config.json
**Location:** `/staticwebapp.config.json`

**Change:**
```json
// Before (Facebook)
"auth": {
  "identityProviders": {
    "facebook": {
      "registration": {
        "appIdSettingName": "FACEBOOK_APP_ID",
        "appSecretSettingName": "FACEBOOK_APP_SECRET"
      }
    }
  }
}

// After (Google)
"auth": {
  "identityProviders": {
    "google": {
      "registration": {
        "clientIdSettingName": "GOOGLE_CLIENT_ID",
        "clientSecretSettingName": "GOOGLE_CLIENT_SECRET"
      }
    }
  }
}
```

**Why:** Tells Azure Static Web Apps to use Google as the authentication provider and expect `GOOGLE_CLIENT_ID` and `GOOGLE_CLIENT_SECRET` environment variables.

---

### 2. AuthService
**Location:** `/src/app/services/auth.service.ts`

**Changes:**

#### Endpoint Update
```typescript
// Before
private readonly LOGIN_ENDPOINT = '/.auth/login/facebook';

// After
private readonly LOGIN_ENDPOINT = '/.auth/login/google';
```

#### Documentation Update
```typescript
// Before
/**
 * Initiate Facebook login flow
 * This redirects to Azure SWA's built-in Facebook authentication endpoint
 */

// After
/**
 * Initiate Google login flow
 * This redirects to Azure SWA's built-in Google authentication endpoint
 */
```

**Why:** The AuthService is the single source of truth for authentication logic. Changing the login endpoint directs users to Google's OAuth instead of Facebook's.

**Architecture Benefit:** No changes needed to the rest of the AuthService - the user info structure, guards, and interceptors all work identically.

---

### 3. LoginComponent TypeScript
**Location:** `/src/app/components/login/login.component.ts`

**Change:**
```typescript
// Before
loginWithFacebook(): void {
  this.isLoading = true;
  this.authService.login();
}

// After
loginWithGoogle(): void {
  this.isLoading = true;
  this.authService.login();
}
```

**Why:** Clarity - the method name now reflects that we're using Google authentication.

---

### 4. LoginComponent Template
**Location:** `/src/app/components/login/login.component.html`

**Changes:**

#### Button Class and Handler
```html
<!-- Before -->
<button class="facebook-btn" (click)="loginWithFacebook()" ...>

<!-- After -->
<button class="google-btn" (click)="loginWithGoogle()" ...>
```

#### Subtitle Text
```html
<!-- Before -->
<p class="subtitle">Sign in with your Facebook account</p>

<!-- After -->
<p class="subtitle">Sign in with your Google account</p>
```

#### Button Text and Icon
```html
<!-- Before -->
Sign in with Facebook
<svg class="facebook-icon" viewBox="0 0 24 24" ...>
  <path d="M24 12.073c0-6.627..." /> <!-- Facebook logo -->
</svg>

<!-- After -->
Sign in with Google
<svg class="google-icon" viewBox="0 0 24 24" ...>
  <path d="M12.48 10.92v3.28h7.84..." /> <!-- Google logo -->
</svg>
```

**Why:** Matches the authentication provider UI to the actual provider being used.

---

### 5. LoginComponent Styling
**Location:** `/src/app/components/login/login.component.css`

**Changes:**

#### Button Styling
```css
/* Before - Facebook Blue */
.facebook-btn {
  background: linear-gradient(135deg, #1877f2 0%, #165bc0 100%);
  color: white;
}

/* After - Google Design */
.google-btn {
  background: white;
  color: #3c4043;
  border: 1px solid #dadce0;
  box-shadow: 0 1px 2px 0 rgba(60, 64, 67, 0.3),
              0 1px 3px 1px rgba(60, 64, 67, 0.15);
}
```

#### Hover States
```css
/* Before - Bright blue shadow */
.facebook-btn:hover:not(:disabled) {
  box-shadow: 0 8px 24px rgba(24, 119, 242, 0.4);
}

/* After - Subtle gray shadow */
.google-btn:hover:not(:disabled) {
  background-color: #f8f9fa;
  border-color: #d2d3d4;
}
```

#### Accent Color
```css
/* Before */
.info-section li:before {
  color: #1877f2; /* Facebook blue */
}

/* After */
.info-section li:before {
  color: #1f2937; /* Dark gray */
}
```

**Why:** Use Google's official button design from their branding guidelines for consistency with user expectations.

---

## Environment Variables

### Before Migration
```bash
FACEBOOK_APP_ID=your-facebook-app-id
FACEBOOK_APP_SECRET=your-facebook-app-secret
```

### After Migration
```bash
GOOGLE_CLIENT_ID=your-google-client-id
GOOGLE_CLIENT_SECRET=your-google-client-secret
```

### How to Get Credentials

See `/GOOGLE_AUTH_SETUP.md` for detailed step-by-step instructions.

**Quick Summary:**
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project
3. Enable Google+ API
4. Create OAuth 2.0 credentials (Web application)
5. Add your app URLs as Authorized Redirect URIs
6. Copy Client ID and Client Secret
7. Set them as environment variables in Azure Static Web Apps

---

## Backward Compatibility

### No Breaking Changes
- All other services, guards, and interceptors remain unchanged
- User info structure is identical
- Dashboard component works without modification
- Authorization checks work the same way

### Migration Path
1. Update files (already done)
2. Get Google OAuth credentials
3. Set environment variables in Azure
4. Redeploy your app
5. All users log in with Google instead of Facebook

---

## Authentication Flow Comparison

### Before (Facebook)
```
User clicks "Sign in with Facebook"
      ↓
AuthService.login() → /.auth/login/facebook
      ↓
User redirects to Facebook OAuth consent
      ↓
User authorizes app
      ↓
Facebook callback → Azure SWA
      ↓
User redirected to /dashboard
      ↓
AuthService fetches user info from /.auth/me
```

### After (Google)
```
User clicks "Sign in with Google"
      ↓
AuthService.login() → /.auth/login/google
      ↓
User redirects to Google OAuth consent
      ↓
User authorizes app
      ↓
Google callback → Azure SWA
      ↓
User redirected to /dashboard
      ↓
AuthService fetches user info from /.auth/me
```

**The authentication flow is identical - only the provider changes.**

---

## Testing Checklist

- [ ] Verify Google credentials are set in Azure Static Web Apps
- [ ] Test login locally: `ng serve` and click "Sign in with Google"
- [ ] Verify Google OAuth dialog appears
- [ ] Verify user is redirected to dashboard after auth
- [ ] Verify user info (name, email) displays correctly
- [ ] Test logout functionality
- [ ] Test that accessing `/dashboard` requires authentication
- [ ] Deploy to Azure and test with production URL
- [ ] Test on mobile devices

---

## Troubleshooting

### Common Issues

**Issue: "Invalid client" error when clicking login**
- Check that GOOGLE_CLIENT_ID and GOOGLE_CLIENT_SECRET are set correctly in Azure
- Verify no extra spaces in the environment variables
- Ensure credentials haven't expired

**Issue: Redirect URI mismatch**
- Get exact SWA URL from Azure Portal
- Ensure it's configured in Google Cloud Console as:
  ```
  https://<your-domain>.azurestaticapps.net/.auth/login/google/callback
  ```
- Check for trailing slashes

**Issue: User info not loading after login**
- Open browser DevTools → Network tab
- Check if `/.auth/me` request succeeds
- Verify user is actually authenticated (check cookies)

For more troubleshooting, see `/GOOGLE_AUTH_SETUP.md`

---

## Rollback Plan

If needed, you can rollback to Facebook by:

1. Reverse the changes in these files:
   - `staticwebapp.config.json`
   - `src/app/services/auth.service.ts`
   - `src/app/components/login/login.component.ts`
   - `src/app/components/login/login.component.html`
   - `src/app/components/login/login.component.css`

2. Restore Facebook environment variables:
   ```bash
   FACEBOOK_APP_ID=...
   FACEBOOK_APP_SECRET=...
   ```

3. Redeploy

Git makes this easy: `git checkout [commit-hash]`

---

## Summary of Benefits

1. **Faster Setup:** 5 minutes vs 30+ minutes
2. **No Approval Wait:** Credentials immediately available
3. **Simpler Configuration:** Single console instead of multiple pages
4. **Better User Experience:** Most users expect Google/Facebook login
5. **More Reliable:** Google's OAuth is very stable
6. **Cost:** Completely free (both were free, but Google has fewer limitations)
7. **Future-Proof:** Google OAuth gets regular updates and improvements

---

## Next Steps

1. Follow `/GOOGLE_AUTH_SETUP.md` to get Google OAuth credentials
2. Set environment variables in Azure Static Web Apps
3. Redeploy your app
4. Test with real users
5. Remove old Facebook OAuth credentials from Google Cloud (if previously configured)

---

## Questions?

Refer to:
- `/GOOGLE_AUTH_SETUP.md` - Detailed Google OAuth setup
- `/AUTH_SERVICE_REFERENCE.md` - How the AuthService works
- `/ARCHITECTURE.md` - Overall application architecture
