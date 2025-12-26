import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';

export interface UserInfo {
  identityProvider: string;
  userId: string;
  userDetails: string;
  claims: Array<{
    typ: string;
    val: string;
  }>;
  clientPrincipal?: {
    identityProvider: string;
    userId: string;
    userDetails: string;
    claims: Array<{
      typ: string;
      val: string;
    }>;
  };
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly AUTH_ME_ENDPOINT = '/.auth/me';
  private readonly LOGIN_ENDPOINT = '/.auth/login/google';
  private readonly LOGOUT_ENDPOINT = '/.auth/logout';

  private userSubject = new BehaviorSubject<UserInfo | null>(null);
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  private isLoadingSubject = new BehaviorSubject<boolean>(true);

  public user$ = this.userSubject.asObservable();
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();
  public isLoading$ = this.isLoadingSubject.asObservable();

  constructor(private http: HttpClient) {
    this.initializeAuth();
  }

  private initializeAuth(): void {
    this.getUserInfo().subscribe(
      (user) => {
        if (user) {
          this.userSubject.next(user);
          this.isAuthenticatedSubject.next(true);
        }
        this.isLoadingSubject.next(false);
      },
      (error) => {
        console.log('User not authenticated', error);
        this.userSubject.next(null);
        this.isAuthenticatedSubject.next(false);
        this.isLoadingSubject.next(false);
      }
    );
  }

  /**
   * Get current user information from Azure SWA auth endpoint
   */
  public getUserInfo(): Observable<UserInfo | null> {
    return this.http.get<{ clientPrincipal: UserInfo | null }>(this.AUTH_ME_ENDPOINT).pipe(
      map((response) => {
        // Azure SWA wraps the user info in a clientPrincipal property
        return response.clientPrincipal || null;
      }),
      catchError((error) => {
        console.error('Error fetching user info:', error);
        return of(null);
      })
    );
  }

  /**
   * Get current user synchronously (useful for templates)
   */
  public getCurrentUser(): UserInfo | null {
    return this.userSubject.value;
  }

  /**
   * Check if user is authenticated
   */
  public isAuthenticated(): boolean {
    return this.isAuthenticatedSubject.value;
  }

  /**
   * Get user's email from claims
   */
  public getUserEmail(): string | null {
    const user = this.userSubject.value;
    if (!user) return null;

    const emailClaim = user.claims?.find(c => c.typ === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress');
    return emailClaim?.val || null;
  }

  /**
   * Get user's name from claims
   */
  public getUserName(): string | null {
    const user = this.userSubject.value;
    if (!user) return null;

    const nameClaim = user.claims?.find(c => c.typ === 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name');
    return nameClaim?.val || user.userDetails || null;
  }

  /**
   * Initiate Google login flow
   * This redirects to Azure SWA's built-in Google authentication endpoint
   */
  public login(): void {
    // Append post-login redirect URL
    const redirectUrl = this.LOGIN_ENDPOINT + '?post_login_redirect_uri=' + encodeURIComponent(window.location.origin + '/dashboard');
    window.location.href = redirectUrl;
  }

  /**
   * Logout the user
   * This clears the session and redirects to home page
   */
  public logout(): void {
    const redirectUrl = this.LOGOUT_ENDPOINT + '?post_logout_redirect_uri=' + encodeURIComponent(window.location.origin);
    window.location.href = redirectUrl;
  }

  /**
   * Refresh user info (useful after login)
   */
  public refreshUserInfo(): Observable<UserInfo | null> {
    return this.getUserInfo().pipe(
      tap((user) => {
        this.userSubject.next(user);
        this.isAuthenticatedSubject.next(!!user);
      })
    );
  }
}
