# Google OAuth Setup Guide for Azure Static Web Apps

This guide walks you through setting up Google OAuth credentials for your Team Manager application running on Azure Static Web Apps. Google's setup is significantly simpler than Facebook's.

## Table of Contents
1. [Create a Google Cloud Project](#create-a-google-cloud-project)
2. [Enable Google OAuth](#enable-google-oauth)
3. [Create OAuth Credentials](#create-oauth-credentials)
4. [Configure Azure Static Web Apps](#configure-azure-static-web-apps)
5. [Test Your Implementation](#test-your-implementation)
6. [Troubleshooting](#troubleshooting)

---

## Create a Google Cloud Project

### Step 1: Access Google Cloud Console

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. If you don't have a Google account, create one first
3. Sign in with your Google account

### Step 2: Create a New Project

1. Click on the project dropdown at the top of the page
2. Click **"NEW PROJECT"**
3. Enter a project name (e.g., "Team Manager" or "Team Manager App")
4. Organization: Leave as default (optional)
5. Click **"CREATE"**

The project will take a few seconds to be created. You'll see a notification when it's ready.

---

## Enable Google OAuth

### Step 1: Enable the Google+ API

1. In the Google Cloud Console, go to **APIs & Services** > **Library**
2. Search for "Google+ API"
3. Click on **"Google+ API"**
4. Click **"ENABLE"**

This enables the Google OAuth functionality for your project.

### Step 2: Create OAuth Consent Screen

1. Go to **APIs & Services** > **OAuth consent screen**
2. Select **"External"** as the User Type
3. Click **"CREATE"**

### Step 3: Fill Out Consent Screen Form

Fill in the following required fields:

**App Name:**
```
Team Manager
```

**User Support Email:**
```
[Your email address]
```

**App Logo:** (Optional)
- You can skip this for now

**Developer Contact Information:**
```
[Your email address]
```

4. Click **"SAVE AND CONTINUE"**

### Step 4: Configure Scopes (Keep Defaults)

1. On the "Scopes" page, you can keep the default settings
2. Click **"SAVE AND CONTINUE"**

### Step 5: Add Test Users (Optional)

1. On the "Test users" page, you can add your email if you want to test as a test user
2. Click **"ADD USERS"** and enter your email
3. Click **"SAVE AND CONTINUE"**

---

## Create OAuth Credentials

### Step 1: Create OAuth 2.0 Credentials

1. Go to **APIs & Services** > **Credentials**
2. Click **"+ CREATE CREDENTIALS"** > **"OAuth client ID"**

You'll be prompted to create an OAuth consent screen first. Make sure you've completed the previous section.

### Step 2: Configure OAuth Client

1. **Application Type:** Select **"Web application"**
2. **Name:** Enter a name (e.g., "Team Manager SWA")

### Step 3: Add Authorized Redirect URIs

This is the most important step. You need to add the Azure Static Web Apps authentication callback URL.

**For local development:**
```
http://localhost:4200/.auth/login/google/callback
```

**For your Azure SWA production deployment:**
```
https://<your-swa-hostname>.azurestaticapps.net/.auth/login/google/callback
```

Replace `<your-swa-hostname>` with your actual Azure Static Web Apps hostname.

To find your SWA hostname:
1. Go to Azure Portal
2. Navigate to your Static Web Apps resource
3. The URL is shown in the Overview section (e.g., `https://gray-coast-abc123.azurestaticapps.net`)

### Step 4: Copy Your Credentials

After clicking **"CREATE"**, you'll see a dialog with:
- **Client ID** (looks like: `123456789-abcdefghijk.apps.googleusercontent.com`)
- **Client Secret** (a long string)

**Copy both values and save them securely.** You'll need these in the next step.

---

## Configure Azure Static Web Apps

### Step 1: Set Environment Variables

Set the following environment variables in your Azure Static Web Apps:

**In Azure Portal:**

1. Go to your Static Web Apps resource
2. Navigate to **Configuration** > **Application settings**
3. Click **"Add"** and add:

| Name | Value |
|------|-------|
| `GOOGLE_CLIENT_ID` | Your Client ID from Step 4 above |
| `GOOGLE_CLIENT_SECRET` | Your Client Secret from Step 4 above |

4. Click **"Save"**

**Alternative: Using Azure CLI**

```bash
az staticwebapp appsettings set \
  --name team-manager \
  --resource-group your-resource-group \
  --setting-names GOOGLE_CLIENT_ID=your-client-id \
                   GOOGLE_CLIENT_SECRET=your-client-secret
```

### Step 2: Verify Configuration

Your `staticwebapp.config.json` should already be configured for Google. Verify it contains:

```json
{
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
}
```

---

## Test Your Implementation

### Step 1: Local Testing

1. Start your Angular development server:
```bash
ng serve
```

2. Navigate to http://localhost:4200/login

3. Click the "Sign in with Google" button

4. You'll be redirected to Google's login page

5. Sign in with your Google account

6. You should be redirected back to your app's dashboard

### Step 2: Production Testing

1. Deploy your app to Azure Static Web Apps
2. Navigate to your SWA URL (e.g., https://gray-coast-abc123.azurestaticapps.net/login)
3. Click "Sign in with Google"
4. Test the full login flow

---

## Troubleshooting

### Issue: "Invalid client" error

**Cause:** Client ID or Client Secret is incorrect or not set in Azure Static Web Apps.

**Solution:**
1. Verify credentials are correctly set in Azure Portal > Configuration > Application settings
2. Ensure no extra spaces or quotes around the values
3. Check that you copied the complete Client ID and Client Secret

### Issue: Redirect URI mismatch error

**Cause:** The redirect URI in Google Cloud Console doesn't match your app's URL.

**Solution:**
1. Get your exact SWA hostname from Azure Portal
2. In Google Cloud Console > Credentials, edit your OAuth client
3. Ensure the Authorized Redirect URI is exactly:
   ```
   https://<your-swa-hostname>.azurestaticapps.net/.auth/login/google/callback
   ```
4. Check for trailing slashes or typos

### Issue: Blank login page or infinite redirect loop

**Cause:** Usually a CORS or routing configuration issue.

**Solution:**
1. Check your browser's Network tab for blocked requests
2. Verify `staticwebapp.config.json` is correctly configured
3. Ensure your Angular build output is in the correct location
4. Check Azure Static Web Apps logs in Azure Portal

### Issue: User information not displaying after login

**Cause:** AuthService might not be fetching user info correctly.

**Solution:**
1. Check browser console for errors
2. Verify `/.auth/me` endpoint is returning user data
3. Ensure AuthService is properly initialized in your app.component.ts

---

## Quick Reference

### Key Endpoints

| Endpoint | Purpose |
|----------|---------|
| `/.auth/login/google` | Initiate Google login |
| `/.auth/login/google/callback` | Google's callback URL (configured in Google Cloud) |
| `/.auth/me` | Get current user information |
| `/.auth/logout` | Log out the current user |

### Files Modified for Google Authentication

1. **staticwebapp.config.json** - Updated auth provider configuration
2. **src/app/services/auth.service.ts** - Updated LOGIN_ENDPOINT to use Google
3. **src/app/components/login/login.component.ts** - Renamed method from `loginWithFacebook()` to `loginWithGoogle()`
4. **src/app/components/login/login.component.html** - Updated button text and icon to Google branding
5. **src/app/components/login/login.component.css** - Updated styling to Google button design

### Comparison: Facebook vs Google Setup

| Aspect | Facebook | Google |
|--------|----------|--------|
| Project Creation | Requires app verification | Simple one-click setup |
| OAuth Setup | 5+ approval steps | 2-3 setup steps |
| Documentation | Scattered across multiple pages | Well-organized in one place |
| Credential Validity | Often requires re-verification | Stable and reliable |
| Redirect URI Config | Multiple locations to configure | Single location |
| **Time to Setup** | **30-60 minutes** | **5-10 minutes** |

---

## Need More Help?

### Official Resources

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [Azure Static Web Apps Authentication](https://learn.microsoft.com/en-us/azure/static-web-apps/authentication-authorization)
- [Azure Static Web Apps Configuration](https://learn.microsoft.com/en-us/azure/static-web-apps/configuration)

### Common Questions

**Q: Can I use the same credentials for both local and production?**

A: No. You need separate OAuth credentials if your local and production URLs are different. The Redirect URI must exactly match the URL where your app is running.

**Q: Do I need to add multiple Redirect URIs if I'm using multiple Azure environments (staging, production)?**

A: Yes. Add all your environment URLs to the Authorized Redirect URIs list in Google Cloud Console.

**Q: How do I remove Google authentication later?**

A: Simply set `identityProviders` to empty object in `staticwebapp.config.json`:
```json
"identityProviders": {}
```

**Q: Is Google OAuth free?**

A: Yes, completely free to use. No billing required.

---

## Summary

You've successfully:
1. Created a Google Cloud Project
2. Enabled Google OAuth APIs
3. Created OAuth 2.0 credentials
4. Configured Azure Static Web Apps with your credentials
5. Updated your Angular app to use Google login

Your Team Manager app now uses Google authentication instead of Facebook, with a much simpler setup process!
