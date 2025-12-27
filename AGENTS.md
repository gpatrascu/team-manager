# AGENTS.md - Team Manager Development Guide

## Build/Test/Lint Commands

```bash
npm start              # Dev server on http://localhost:4200 (ng serve)
npm run build          # Production build (outputs to dist/team-manager)
npm run watch          # Watch mode build for development
npm test               # Run all tests with Karma/Jasmine
npm test -- --watch   # Run tests in watch mode
npm test -- --code-coverage  # Run with coverage report
npm run ng -- test --include='**/feature.spec.ts'  # Run specific test
```

## Architecture & Project Structure

**Frontend Framework**: Angular 18 (standalone components, no NgModules)
- **Language**: TypeScript 5.5 with strict mode enabled
- **Build Tool**: Angular CLI 18 with Vite
- **Auth**: Azure Static Web Apps (SWA) built-in Facebook/Google OAuth
- **HTTP**: HttpClient with custom interceptor for auth (401/403 handling)
- **State**: RxJS observables (BehaviorSubjects in AuthService)
- **Routing**: Standalone routes with authGuard protection
- **Session**: HttpOnly, Secure session cookies managed by Azure SWA

**Directory Structure**:
```
src/
├── app/
│   ├── components/      # Login, Dashboard components
│   ├── services/        # AuthService (user state, login/logout)
│   ├── guards/          # authGuard (protects /dashboard route)
│   ├── interceptors/    # authInterceptor (handles 401/403, auth headers)
│   ├── app.component.ts # Root, shows loading during auth check
│   └── app.routes.ts    # Routes config: /login, /dashboard, etc.
├── public/              # Static assets
└── index.html           # Entry point
```

**Key Services & APIs**:
- `AuthService`: Manages user$ (BehaviorSubject), isAuthenticated$, isLoading$ observables
- `/.auth/me`: Gets current user from Azure SWA (returns `{clientPrincipal: UserInfo}`)
- `/.auth/login/google`: Initiates Google OAuth flow (redirects)
- `/.auth/logout`: Clears session, redirects to home
- `UserInfo` interface: Contains identityProvider, userId, userDetails, claims array

## Code Style & Conventions

**TypeScript Config**: `strict: true`, `noImplicitOverride: true`, `noPropertyAccessFromIndexSignature: true`

**Imports & Organization**:
- Group imports: Angular/rxjs first, then services, then local types
- Example: `import { Injectable } from '@angular/core'; import { HttpClient } from '@angular/common/http';`
- Use absolute paths (not relative `../`)

**Naming Conventions**:
- Components: `ComponentNameComponent` (PascalCase)
- Services: `ServiceNameService` (PascalCase)
- Files: `kebab-case.component.ts`, `kebab-case.service.ts`
- Observable properties: `user$`, `isLoading$` ($ suffix per RxJS convention)
- Private fields: `private userSubject` (camelCase with private keyword)
- Constants: `private readonly AUTH_ME_ENDPOINT = '/.auth/me'` (UPPER_SNAKE_CASE)

**Type Safety**:
- Export interfaces near top of file (e.g., `UserInfo` in auth.service.ts)
- Use strict null checks; check `user?.claims?.find()` pattern
- Prefer observables over promises for consistency
- Use `Observable<T | null>` for optional user data

**Error Handling**:
- Use `.pipe(catchError())` to handle HTTP errors gracefully
- Log errors with `console.error()` for debugging
- Return `of(null)` in catchError to prevent stream breaking
- Guard checks like `if (!user) return null;` for null safety

**Angular Patterns**:
- Standalone components with `standalone: true`
- Dependency injection via constructor (`constructor(private authService: AuthService)`)
- Observables in templates: async pipe `{{ user$ | async }}`
- Guard return `true/false` or `Router.navigate()`
- Private methods prefixed with underscore pattern not used; use `private` keyword
