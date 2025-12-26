import { Injectable } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';
import { map, take, filter } from 'rxjs/operators';

/**
 * Guard to protect routes that require authentication
 * Redirects unauthenticated users to login page
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Wait for loading to complete, then check authentication
  return authService.isLoading$.pipe(
    // Filter out loading states - only continue when loading is false
    filter((isLoading) => !isLoading),
    // Take the first non-loading state
    take(1),
    // Check authentication status
    map(() => {
      const isAuthenticated = authService.isAuthenticated();

      if (isAuthenticated) {
        return true;
      } else {
        // Redirect to login and preserve the intended URL
        router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
        return false;
      }
    })
  );
};

/**
 * Alternative guard using Angular 14+ functional style
 * For routes that need to check authentication status
 */
@Injectable({ providedIn: 'root' })
export class AuthGuardService {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): Observable<boolean> {
    return this.authService.isAuthenticated$.pipe(
      take(1),
      map((isAuthenticated) => {
        if (!isAuthenticated) {
          this.router.navigate(['/login']);
          return false;
        }
        return true;
      })
    );
  }
}

// Import Observable for type
import { Observable } from 'rxjs';
