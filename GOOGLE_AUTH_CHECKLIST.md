# Google Authentication Migration - Quick Checklist

## Code Changes (COMPLETED)
- [x] Updated `staticwebapp.config.json` - now uses Google provider
- [x] Updated `AuthService` - login endpoint changed to `/.auth/login/google`
- [x] Updated `LoginComponent` - method renamed to `loginWithGoogle()`
- [x] Updated login template - button shows "Sign in with Google" with Google logo
- [x] Updated login styles - Google button design applied

## Before You Deploy

### Step 1: Get Google OAuth Credentials (5-10 minutes)
- [ ] Open [Google Cloud Console](https://console.cloud.google.com/)
- [ ] Create a new project (or use existing one)
- [ ] Enable Google+ API
- [ ] Go to Credentials > Create OAuth 2.0 Client ID
- [ ] Application Type: **Web application**
- [ ] Add Authorized Redirect URIs:
  - For **local testing**: `http://localhost:4200/.auth/login/google/callback`
  - For **production**: `https://<your-swa-hostname>.azurestaticapps.net/.auth/login/google/callback`
- [ ] Copy your **Client ID** and **Client Secret**

**Where to find your SWA hostname:**
- Azure Portal → Your Static Web Apps resource → Overview → URL

### Step 2: Set Environment Variables in Azure (2 minutes)
- [ ] Go to Azure Portal
- [ ] Navigate to your Static Web Apps resource
- [ ] Settings → Configuration → Application settings
- [ ] Add two new settings:

| Name | Value |
|------|-------|
| `GOOGLE_CLIENT_ID` | Paste your Client ID |
| `GOOGLE_CLIENT_SECRET` | Paste your Client Secret |

- [ ] Click Save

### Step 3: Deploy (3-5 minutes)
- [ ] Commit your code changes: `git add . && git commit -m "Switch to Google authentication"`
- [ ] Push to main branch: `git push origin main`
- [ ] Azure Static Web Apps will automatically build and deploy
- [ ] Wait for deployment to complete (check Azure Portal > Deployments)

### Step 4: Test Locally (2 minutes)
```bash
# Terminal 1: Start your app
ng serve

# Open browser to http://localhost:4200/login
# Click "Sign in with Google"
# Verify Google OAuth dialog appears
# Sign in and verify redirect to dashboard
```

### Step 5: Test in Production (2 minutes)
- [ ] Open your SWA URL: `https://<your-swa-hostname>.azurestaticapps.net/login`
- [ ] Click "Sign in with Google"
- [ ] Complete the login flow
- [ ] Verify dashboard displays user information

## Verification Checklist

### Login Page
- [ ] "Sign in with Google" button is visible
- [ ] Google logo is displayed
- [ ] Button styling looks correct

### Authentication Flow
- [ ] Clicking button redirects to Google OAuth
- [ ] User can sign in with Google account
- [ ] After authorization, redirected back to dashboard
- [ ] User info (name, email) displays correctly

### Security
- [ ] Users cannot access `/dashboard` without authentication
- [ ] Logout functionality works correctly
- [ ] Session persists on page reload
- [ ] Auth tokens are properly managed

### Edge Cases
- [ ] Works on mobile devices
- [ ] Works in incognito/private mode
- [ ] Multiple users can log in and out sequentially
- [ ] No console errors in DevTools

## Rollback Plan (If Needed)

If something goes wrong, you can quickly rollback:

```bash
# Find the previous commit hash
git log --oneline | head -5

# Checkout the previous state
git revert HEAD

# Push the revert
git push origin main
```

OR if critical:

```bash
git reset --hard <previous-commit-hash>
git push origin main --force
```

Then restore your old Facebook environment variables in Azure.

## Common Issues & Solutions

### "Invalid client" error
- [ ] Check GOOGLE_CLIENT_ID is set correctly (no extra spaces)
- [ ] Check GOOGLE_CLIENT_SECRET is set correctly (no extra spaces)
- [ ] Wait 2-3 minutes after setting variables for them to propagate
- [ ] Try clearing browser cache and cookies

### Redirect URI mismatch error
- [ ] Get exact URL from Azure Portal
- [ ] In Google Cloud Console, edit OAuth client
- [ ] Verify Redirect URI matches exactly (check for trailing slashes)
- [ ] Format should be: `https://<hostname>.azurestaticapps.net/.auth/login/google/callback`

### Button doesn't work
- [ ] Check browser console for errors (F12 → Console tab)
- [ ] Verify staticwebapp.config.json has correct Google provider settings
- [ ] Check that Angular compiled successfully (`ng build`)

### User info not showing
- [ ] Open DevTools → Network tab
- [ ] After login, check if `/.auth/me` request succeeds
- [ ] Verify response contains user info
- [ ] Check AuthService.getUserInfo() is being called

## Documentation References

For more detailed information, see:

- **`GOOGLE_AUTH_SETUP.md`** - Complete step-by-step Google OAuth setup guide
- **`MIGRATION_FACEBOOK_TO_GOOGLE.md`** - Detailed migration documentation
- **`AUTH_SERVICE_REFERENCE.md`** - How authentication works in your app
- **`ARCHITECTURE.md`** - Overall application architecture

## Time Estimates

| Task | Time |
|------|------|
| Get Google credentials | 5-10 min |
| Set Azure environment variables | 2 min |
| Deploy code | 3-5 min |
| Local testing | 2 min |
| Production testing | 2 min |
| **Total** | **15-25 min** |

## Quick Links

- [Google Cloud Console](https://console.cloud.google.com/)
- [Azure Portal](https://portal.azure.com/)
- [Google OAuth Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Azure SWA Authentication Docs](https://learn.microsoft.com/en-us/azure/static-web-apps/authentication-authorization)

## Support

If you get stuck:

1. Check the "Common Issues & Solutions" section above
2. Review the detailed setup guide in `GOOGLE_AUTH_SETUP.md`
3. Check Azure Static Web Apps logs in Azure Portal
4. Check browser DevTools console for error messages
5. Verify all environment variables are correctly set (no typos)

---

## Summary

You're ready to go! Just:
1. Get Google credentials (5 min)
2. Set 2 environment variables in Azure (2 min)
3. Deploy your code (5 min)
4. Test (5 min)
5. Done!

The code is already updated. Google authentication is significantly simpler than Facebook, so you should be up and running very quickly.

**Good luck!**
