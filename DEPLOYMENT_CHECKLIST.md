# Deployment Checklist - Facebook Authentication

Complete checklist to deploy Facebook authentication to your Azure Static Web Apps.

## Pre-Deployment Phase

### Facebook Developer Setup

- [ ] Create Facebook Developer account at https://developers.facebook.com
- [ ] Create new Facebook App
- [ ] Add "Facebook Login" product
- [ ] Go to Facebook Login Settings
- [ ] Add redirect URIs:
  - [ ] Production: `https://orange-cliff-0c10d4603.azurestaticapps.net/.auth/login/facebook/callback`
  - [ ] Development: `http://localhost:4200/.auth/login/facebook/callback`
- [ ] Verify app is in Development or Live mode (not Sandbox)
- [ ] Copy and save **App ID** (FACEBOOK_APP_ID)
- [ ] Copy and save **App Secret** (FACEBOOK_APP_SECRET)
- [ ] Create test user for development

### Code Files Verification

All required files should exist in your project:

#### Core Services
- [ ] `/src/app/services/auth.service.ts` (main authentication service)
- [ ] `/src/app/guards/auth.guard.ts` (route protection)
- [ ] `/src/app/interceptors/auth.interceptor.ts` (HTTP error handling)

#### Components
- [ ] `/src/app/components/login/login.component.ts`
- [ ] `/src/app/components/login/login.component.html`
- [ ] `/src/app/components/login/login.component.css`
- [ ] `/src/app/components/dashboard/dashboard.component.ts`
- [ ] `/src/app/components/dashboard/dashboard.component.html`
- [ ] `/src/app/components/dashboard/dashboard.component.css`

#### Configuration
- [ ] `/src/app/app.routes.ts` (route definitions)
- [ ] `/src/app/app.config.ts` (Angular configuration)
- [ ] `/src/app/app.component.ts` (updated main component)
- [ ] `/staticwebapp.config.json` (Azure SWA configuration)

#### Documentation
- [ ] `/FACEBOOK_AUTH_SETUP.md` (setup instructions)
- [ ] `/API_INTEGRATION_GUIDE.md` (API usage guide)
- [ ] `/AUTH_SERVICE_REFERENCE.md` (service API reference)
- [ ] `/ARCHITECTURE.md` (architecture diagrams)
- [ ] `/IMPLEMENTATION_SUMMARY.md` (implementation overview)
- [ ] `/QUICK_START.md` (quick start guide)
- [ ] `/DEPLOYMENT_CHECKLIST.md` (this file)

### Local Build Verification

```bash
# From /Users/g.patrascu/repos/learning/team-manager
```

- [ ] Run `npm install` - all dependencies resolve
- [ ] Run `npm run build` - build completes without errors
- [ ] Check `/dist/team-manager` directory exists
- [ ] Verify `index.html` and assets are present
- [ ] No TypeScript compilation errors
- [ ] No console warnings about deprecated features

### Local Testing (Optional but Recommended)

- [ ] Install SWA CLI: `npm install -g @azure/static-web-apps-cli`
- [ ] Create `swa-cli.config.json` in project root
- [ ] Run `swa start http://localhost:4200`
- [ ] Visit http://localhost:4200 in browser
- [ ] Verify login page loads at /login
- [ ] Click "Sign in with Facebook" button
- [ ] Complete Facebook login flow
- [ ] See dashboard with user info after login
- [ ] Click "Logout" button
- [ ] Verify redirected back to home page
- [ ] Try accessing /dashboard without login
- [ ] Verify redirected to /login (route guard works)

## Azure Portal Phase

### Configure Environment Variables

1. Navigate to Azure Portal: https://portal.azure.com
2. Find your Static Web App: `orange-cliff-0c10d4603`
3. Go to **Settings > Configuration**

- [ ] Click "Add" to create new setting
- [ ] **Application Setting 1**:
  - [ ] Name: `FACEBOOK_APP_ID`
  - [ ] Value: `<your-facebook-app-id>`
  - [ ] Click OK

- [ ] Click "Add" to create new setting
- [ ] **Application Setting 2**:
  - [ ] Name: `FACEBOOK_APP_SECRET`
  - [ ] Value: `<your-facebook-app-secret>`
  - [ ] Click OK

- [ ] Click **Save** button
- [ ] Wait for configuration to update (usually takes 1-2 minutes)

### Verify Configuration

- [ ] Go back to Configuration section
- [ ] Confirm both settings are listed
- [ ] No error messages displayed

## Deployment Phase

### Build and Commit

```bash
# From /Users/g.patrascu/repos/learning/team-manager
```

- [ ] Run `npm run build` to create production build
- [ ] Verify no build errors
- [ ] Check `/dist/team-manager` contains all files
- [ ] Run `git status` to see changes
- [ ] Review changes are correct (authentication files added/modified)

- [ ] Stage changes: `git add .`
- [ ] Commit: `git commit -m "Add: Facebook authentication with Azure SWA"`
- [ ] Push to main branch: `git push origin main`

### Monitor GitHub Actions Workflow

- [ ] Go to GitHub repository
- [ ] Click **Actions** tab
- [ ] Find the deployment workflow triggered by your push
- [ ] Wait for workflow to complete (usually 3-5 minutes)
- [ ] Verify workflow shows green checkmark (success)
- [ ] Check workflow logs for any errors:
  - [ ] Angular build succeeds
  - [ ] No TypeScript compilation errors
  - [ ] SWA deployment completes
  - [ ] Artifacts uploaded to Azure

### Verify Azure SWA Deployment

1. Go to Azure Portal
2. Navigate to your Static Web App resource
3. Click **Overview**

- [ ] Status shows "Running" (green)
- [ ] URL displays: `https://orange-cliff-0c10d4603.azurestaticapps.net`
- [ ] **Recent deployments** shows your new deployment
- [ ] Deployment status is "Succeeded"
- [ ] No deployment errors

## Post-Deployment Testing Phase

### Test Live Application

- [ ] Open https://orange-cliff-0c10d4603.azurestaticapps.net in browser
- [ ] Verify login page loads (not errors)
- [ ] Verify layout looks correct
- [ ] Verify "Sign in with Facebook" button is visible
- [ ] Click "Sign in with Facebook" button
- [ ] Verify redirected to Facebook login page
- [ ] Login with your Facebook test user
- [ ] Verify redirected back to application
- [ ] Verify on /dashboard page (URL shows `.../dashboard`)
- [ ] Verify user info displays:
  - [ ] User name/email shows
  - [ ] Claims are displayed
  - [ ] Identity provider shows "facebook"

### Test Logout

- [ ] Click "Logout" button on dashboard
- [ ] Verify redirected to home page
- [ ] Verify session cookie is cleared (check DevTools)
- [ ] Try accessing /dashboard directly
- [ ] Verify redirected to /login
- [ ] Verify login button is functional again

### Test Protected Routes

- [ ] Open DevTools (F12 > Application > Cookies)
- [ ] Logout and verify authentication cookie is gone
- [ ] Try accessing https://orange-cliff-0c10d4603.azurestaticapps.net/dashboard directly
- [ ] Verify redirected to /login page
- [ ] Login again
- [ ] Verify can access /dashboard
- [ ] Verify user info loads correctly

### Test API Integration (if you have APIs)

- [ ] Make API call from component: `this.http.get('/api/teams')`
- [ ] Verify API receives authentication cookie
- [ ] Verify API returns data (or appropriate error)
- [ ] Verify 401 errors redirect to login

### Test Cross-Browser Compatibility

- [ ] Test in Chrome
- [ ] Test in Firefox
- [ ] Test in Safari
- [ ] Test in Edge
- [ ] Test on mobile browser (iOS Safari or Chrome Mobile)
- [ ] Verify login works in all browsers
- [ ] Verify session persists across page refreshes

### Test Different Scenarios

- [ ] **New user**: Logout, close browser, open URL - verify login page shows
- [ ] **Session timeout**: Wait for session to expire - verify redirected to login
- [ ] **Multiple tabs**: Login in one tab - verify other tabs recognize login
- [ ] **Back button**: After login, click back button - verify not redirected to Facebook
- [ ] **Bookmarked /dashboard**: Try visiting bookmarked dashboard URL when logged out

## Monitoring Phase

### Set Up Alerts (Optional)

- [ ] Go to Azure Portal > Static Web App > Monitoring
- [ ] Set up alert for deployment failures
- [ ] Set up alert for high error rates
- [ ] Set up alert for high latency

### Monitor Logs

- [ ] Go to Azure Portal > Static Web App > Logs
- [ ] Verify no 401 errors for authenticated requests
- [ ] Verify no 500 errors in application
- [ ] Check for authentication-related errors
- [ ] Verify /.auth/me endpoint is working

### Monitor Traffic

- [ ] Check if users are accessing your application
- [ ] Verify login flow is being used
- [ ] Monitor for any errors in the logs

## Documentation Phase

### Update Application Docs

- [ ] Add Facebook authentication to main README.md
- [ ] Document how to set up development environment
- [ ] Document environment variables required
- [ ] Add architecture overview to team wiki

### Update Team Documentation

- [ ] Notify team of new authentication system
- [ ] Share QUICK_START.md with team
- [ ] Schedule training if needed
- [ ] Document support process for authentication issues

## Post-Deployment Maintenance

### Week 1

- [ ] Monitor error logs daily
- [ ] Check user login success rates
- [ ] Verify no performance issues
- [ ] Be available for bug fixes
- [ ] Monitor Azure costs (should be minimal)

### Ongoing

- [ ] Keep Facebook app credentials secure
- [ ] Rotate credentials periodically (optional, best practice)
- [ ] Monitor for Azure SWA updates
- [ ] Keep Angular dependencies updated
- [ ] Review authentication logs monthly

## Troubleshooting Checklist

If issues occur, check these items:

### Login Not Working
- [ ] Verify FACEBOOK_APP_ID is set in Azure Portal
- [ ] Verify FACEBOOK_APP_SECRET is set in Azure Portal
- [ ] Verify redirect URI in Facebook app matches exactly
- [ ] Check Azure Portal logs for auth errors
- [ ] Verify staticwebapp.config.json has auth config
- [ ] Test in private browsing mode (rules out cookie issues)
- [ ] Check Facebook app is in Development or Live mode

### User Info Not Displaying
- [ ] Open DevTools > Network tab
- [ ] Check that /.auth/me request succeeds (200 status)
- [ ] Verify response includes user claims
- [ ] Check browser console for JavaScript errors
- [ ] Verify AuthService is being injected correctly

### Route Guard Not Working
- [ ] Verify authGuard is applied to /dashboard route
- [ ] Check that authGuard is imported in app.routes.ts
- [ ] Test by logging out and trying to access /dashboard
- [ ] Check browser console for errors

### Logout Not Working
- [ ] Verify logout button calls authService.logout()
- [ ] Check that /.auth/logout endpoint is accessible
- [ ] Verify session cookie is cleared after logout
- [ ] Check browser DevTools > Cookies for authentication cookie

### Environment Variables Not Working
- [ ] Verify both FACEBOOK_APP_ID and FACEBOOK_APP_SECRET are set
- [ ] Wait 2-3 minutes after setting variables for changes to take effect
- [ ] Redeploy application after setting variables
- [ ] Check Azure Portal Logs for environment variable errors

## Rollback Plan

If you need to rollback the deployment:

1. Go to Azure Portal > Static Web App > Deployment
2. Find the previous successful deployment
3. Click the "..." menu
4. Select "Redeploy" to restore previous version
5. Or use GitHub Actions to deploy a previous commit

## Sign-Off Checklist

- [ ] All tests pass
- [ ] No console errors in production
- [ ] Users can login via Facebook
- [ ] Protected routes are secure
- [ ] API integration works (if applicable)
- [ ] Team is notified of changes
- [ ] Documentation is complete
- [ ] Ready for production use

## Final Verification

Before considering this complete:

- [ ] Production URL loads without errors
- [ ] Facebook login works end-to-end
- [ ] User info displays after login
- [ ] Logout works correctly
- [ ] Protected routes are enforced
- [ ] Mobile browsers work correctly
- [ ] All team members can access documentation
- [ ] Support process is documented

## Deployment Summary

- **Deployment Date**: _______________
- **Deployed By**: _______________
- **Facebook App ID**: _______________
- **Azure SWA URL**: https://orange-cliff-0c10d4603.azurestaticapps.net
- **Test Users Created**: _______________
- **Known Issues**: None
- **Notes**: _______________

---

## Quick Reference Links

- Azure Portal: https://portal.azure.com
- Facebook Developers: https://developers.facebook.com
- GitHub Repository: [Your repo URL]
- SWA Documentation: https://learn.microsoft.com/en-us/azure/static-web-apps/
- Facebook Login Docs: https://developers.facebook.com/docs/facebook-login

## Support Contacts

- Azure Support: [Your Azure support contact]
- Facebook Support: https://developers.facebook.com/support
- Team Lead: _______________
- Backup Owner: _______________

---

**Status**: [ ] Ready for Production | [ ] Needs Fixes | [ ] Testing in Progress

**Signed Off**: _______________  Date: _______________
