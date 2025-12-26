import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

/**
 * HTTP Interceptor for handling authentication-related requests and errors
 * - Excludes auth endpoints from interceptor logic (they're handled by Azure SWA)
 * - Handles 401/403 responses by redirecting to login
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  // Skip interceptor for auth endpoints - they're managed by Azure SWA
  const isAuthEndpoint = req.url.includes('/.auth/');

  if (isAuthEndpoint) {
    return next(req);
  }

  // Pass through the request
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 || error.status === 403) {
        // User is not authenticated or unauthorized
        // Redirect to login page
        router.navigate(['/login'], {
          queryParams: { returnUrl: router.url }
        });
      }

      return throwError(() => error);
    })
  );
};
