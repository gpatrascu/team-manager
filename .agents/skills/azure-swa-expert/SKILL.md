---
name: azure-swa-expert
description: Configures Azure Static Web Apps routing, authentication, deployment, and troubleshoots SWA-specific issues. Use for SWA-related problems, auth setup, route configuration, and production deployment.
---

# Azure Static Web Apps Expert

Specializes in Azure Static Web Apps (SWA) configuration, routing, authentication, and troubleshooting for this team-manager project.

## Key Expertise

- **Authentication**: Google/Facebook OAuth via SWA's built-in identity providers
- **Routing**: `staticwebapp.config.json` route rules, fallback handling, role-based access
- **Session Management**: HttpOnly cookies, session TTL, logout flows
- **Deployment**: GitHub Actions workflows, environment variables, preview environments
- **Troubleshooting**: 404 errors, auth redirects, cookie issues, CORS problems

## Common Tasks

### Route Configuration Issues
- Adding authenticated/unauthenticated route rules
- Setting up SPA fallback with `navigationFallback`
- Role-based route protection (`allowedRoles`)
- Wildcard patterns for nested routes

### Authentication Problems
- Login redirects not working (Azure login endpoint, post_login_redirect_uri)
- Session cookies missing or invalid
- OAuth provider setup (clientId, clientSecret env vars)
- OIDC configuration for custom providers

### Deployment & CI/CD
- GitHub Actions SWA deployment workflow
- Environment variable management in Azure portal
- Preview environment setup for pull requests
- Build output path configuration

### Quick Fixes
- **404 on deep routes**: Add `navigationFallback.rewrite: "/index.html"` and exclude static assets
- **Auth not persisting**: Check session cookie is HttpOnly/Secure, verify /.auth/me endpoint
- **Login loop**: Ensure post_login_redirect_uri points to valid app route, not external URL
- **Role checks failing**: Verify `allowedRoles` match SWA's claims (usually ["anonymous"] or ["authenticated"])

## When to Use

Use this skill when:
- Debugging SWA-specific routing or auth issues
- Configuring `staticwebapp.config.json`
- Setting up OAuth providers in Azure portal
- Troubleshooting 404 errors with Angular SPA routing
- Managing deployment workflows
- Handling session/cookie problems

## Reference

- **SWA Config Docs**: https://learn.microsoft.com/en-us/azure/static-web-apps/configuration
- **Auth Setup**: https://learn.microsoft.com/en-us/azure/static-web-apps/authentication-authorization
- **Routes**: https://learn.microsoft.com/en-us/azure/static-web-apps/routes
