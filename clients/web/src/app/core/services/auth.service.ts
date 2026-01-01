import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError, of } from 'rxjs';
import { map, tap, catchError, switchMap } from 'rxjs/operators';

import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { environment } from '../../../environments/environment';
import { User, UserProfile } from '../../models';
import {
  TokenPair,
  AuthState,
  AuthUserInfo,
  LoginRequest,
  VerifyOtpRequest,
  OtpVerifyResult,
  RegisterStartRequest,
  RegisterStartResult,
  RegisterCompleteRequest,
  RegisterCompleteResult,
  PasskeyRegistrationOptions,
  PasskeyAuthenticationOptions,
  PasskeyRegistrationResponse,
  PasskeyAuthenticationResponse,
  PasskeyCredential,
  PasskeyListResponse,
  PasskeyRegisterStartResult,
  PasskeyRegistrationStartData,
  PasskeyAuthenticateStartResult,
  PasskeyAuthenticationStartData,
  PasskeyRegisterCompleteRequest,
  PasskeyAuthenticateCompleteRequest,
} from '../../models';

/**
 * Authentication service for managing user authentication state,
 * including passkey (WebAuthn), email OTP, and token management.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly api = inject(ApiService);
  private readonly storage = inject(StorageService);
  private readonly router = inject(Router);

  // Reactive state using signals
  private readonly _isLoading = signal(false);
  private readonly _user = signal<AuthUserInfo | null>(null);
  private readonly _error = signal<string | null>(null);

  // Public readonly signals
  readonly isLoading = this._isLoading.asReadonly();
  readonly user = this._user.asReadonly();
  readonly error = this._error.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);

  // Token refresh state
  private refreshTokenTimeout?: ReturnType<typeof setTimeout>;

  constructor() {
    // Initialize auth state from storage
    this.initializeFromStorage();
  }

  /**
   * Initialize authentication state from stored tokens
   */
  private initializeFromStorage(): void {
    const tokens = this.getStoredTokens();
    if (tokens) {
      const user = this.decodeToken(tokens.accessToken);
      if (user && !this.isTokenExpired(tokens.accessToken)) {
        this._user.set(user);
        this.scheduleTokenRefresh(tokens);
      } else {
        // Token expired, try refresh
        this.refreshToken().subscribe();
      }
    }
  }

  // ============================================
  // Email OTP Authentication
  // ============================================

  /**
   * Request email OTP for login
   * @param email User email
   */
  requestOtp(email: string): Observable<{ message: string }> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.post<{ message: string }>('auth/otp/request', { email } as LoginRequest).pipe(
      tap(() => this._isLoading.set(false)),
      catchError((error) => {
        this._isLoading.set(false);
        this._error.set(error.error?.message || 'Failed to send OTP');
        return throwError(() => error);
      })
    );
  }

  /**
   * Verify email OTP and complete login
   * @param email User email
   * @param code OTP code
   */
  verifyOtp(email: string, code: string): Observable<OtpVerifyResult> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.post<OtpVerifyResult>('auth/otp/verify', { email, code } as VerifyOtpRequest).pipe(
      tap((result) => {
        if (result.success && result.auth) {
          this.handleAuthSuccess(result.auth);
        } else {
          this._isLoading.set(false);
          this._error.set(result.error || 'Invalid or expired OTP');
        }
      }),
      catchError((error) => {
        this._isLoading.set(false);
        this._error.set(error.error?.message || 'Invalid or expired OTP');
        return throwError(() => error);
      })
    );
  }

  // ============================================
  // Passkey (WebAuthn) Authentication
  // ============================================

  /**
   * Check if WebAuthn is supported in the current browser
   */
  isPasskeySupported(): boolean {
    return (
      typeof window !== 'undefined' &&
      window.PublicKeyCredential !== undefined &&
      typeof window.PublicKeyCredential === 'function'
    );
  }

  /**
   * Start passkey registration process
   * @param email User email for the passkey
   * @returns Observable with parsed WebAuthn options and session ID
   */
  startPasskeyRegistration(email: string): Observable<PasskeyRegistrationStartData> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.post<PasskeyRegisterStartResult>('auth/passkey/register/start', { email }).pipe(
      map((result) => ({
        options: JSON.parse(result.options) as PasskeyRegistrationOptions,
        sessionId: result.sessionId,
      })),
      tap(() => this._isLoading.set(false)),
      catchError((error) => {
        this._isLoading.set(false);
        this._error.set(error.error?.message || 'Failed to start passkey registration');
        return throwError(() => error);
      })
    );
  }

  /**
   * Complete passkey registration with credential response
   * @param sessionId Session ID from registration start
   * @param response Passkey credential response from browser
   * @param displayName Optional display name for the passkey
   */
  completePasskeyRegistration(
    sessionId: string,
    response: PasskeyRegistrationResponse,
    displayName?: string
  ): Observable<{ success: boolean; credentialId?: string; error?: string }> {
    this._isLoading.set(true);

    const request: PasskeyRegisterCompleteRequest = {
      sessionId,
      attestationResponse: response,
      displayName,
    };

    return this.api.post<{ success: boolean; credentialId?: string; error?: string }>(
      'auth/passkey/register/complete',
      request
    ).pipe(
      tap(() => this._isLoading.set(false)),
      catchError((error) => {
        this._isLoading.set(false);
        this._error.set(error.error?.message || 'Failed to complete passkey registration');
        return throwError(() => error);
      })
    );
  }

  /**
   * Start passkey authentication (login)
   * @param email Optional email to limit credentials
   * @returns Observable with parsed WebAuthn options and session ID
   */
  startPasskeyAuthentication(email?: string): Observable<PasskeyAuthenticationStartData> {
    this._isLoading.set(true);
    this._error.set(null);

    const body = email ? { email } : {};
    return this.api.post<PasskeyAuthenticateStartResult>('auth/passkey/authenticate/start', body).pipe(
      map((result) => ({
        options: JSON.parse(result.options) as PasskeyAuthenticationOptions,
        sessionId: result.sessionId,
      })),
      tap(() => this._isLoading.set(false)),
      catchError((error) => {
        this._isLoading.set(false);
        this._error.set(error.error?.message || 'Failed to start passkey authentication');
        return throwError(() => error);
      })
    );
  }

  /**
   * Complete passkey authentication with credential response
   * @param sessionId Session ID from authentication start
   * @param response Passkey authentication response from browser
   */
  completePasskeyAuthentication(
    sessionId: string,
    response: PasskeyAuthenticationResponse
  ): Observable<{ success: boolean; auth?: TokenPair; error?: string }> {
    this._isLoading.set(true);

    const request: PasskeyAuthenticateCompleteRequest = {
      sessionId,
      assertionResponse: response,
    };

    return this.api.post<{ success: boolean; auth?: TokenPair; error?: string }>(
      'auth/passkey/authenticate/complete',
      request
    ).pipe(
      tap((result) => {
        if (result.success && result.auth) {
          this.handleAuthSuccess(result.auth);
        } else {
          this._isLoading.set(false);
        }
      }),
      catchError((error) => {
        this._isLoading.set(false);
        this._error.set(error.error?.message || 'Passkey authentication failed');
        return throwError(() => error);
      })
    );
  }

  /**
   * Get list of registered passkeys for current user
   */
  getPasskeys(): Observable<PasskeyCredential[]> {
    return this.api.get<PasskeyListResponse>('auth/passkey/list').pipe(
      map((response) => this.mapPasskeyResponse(response))
    );
  }

  /**
   * Get list of registered passkeys for a specific user (admin only)
   * @param familyId Family ID
   * @param userId User ID to fetch passkeys for
   */
  getPasskeysForUser(familyId: string, userId: string): Observable<PasskeyCredential[]> {
    return this.api.get<PasskeyListResponse>(`users/family/${familyId}/${userId}/passkeys`).pipe(
      map((response) => this.mapPasskeyResponse(response))
    );
  }

  /**
   * Maps the API passkey list response to frontend PasskeyCredential format
   */
  private mapPasskeyResponse(response: PasskeyListResponse): PasskeyCredential[] {
    if (!response?.passkeys) {
      return [];
    }
    return response.passkeys.map((p) => ({
      id: p.id,
      name: p.displayName || 'Passkey',
      createdAt: p.registeredAt,
      lastUsedAt: p.lastUsedAt,
      transports: p.transports as AuthenticatorTransport[] | undefined,
    }));
  }

  /**
   * Delete a passkey
   * @param credentialId Passkey credential ID
   */
  deletePasskey(credentialId: string): Observable<void> {
    return this.api.delete<void>(`auth/passkey/${credentialId}`);
  }

  /**
   * Delete a passkey for a specific user (admin only)
   * @param familyId Family ID
   * @param userId User ID
   * @param credentialId Passkey credential ID
   */
  deletePasskeyForUser(familyId: string, userId: string, credentialId: string): Observable<void> {
    return this.api.delete<void>(`users/family/${familyId}/${userId}/passkeys/${credentialId}`);
  }

  // ============================================
  // User Registration (2-Step with Email Verification)
  // ============================================

  /**
   * Start the registration process by sending an OTP to verify email ownership.
   * Step 1 of 2: Validates email, sends verification code, returns session ID.
   * @param request Registration details
   * @returns Observable with session ID and verification status
   */
  startRegistration(request: RegisterStartRequest): Observable<RegisterStartResult> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.post<RegisterStartResult>('auth/register/start', request).pipe(
      tap(() => this._isLoading.set(false)),
      catchError((error) => {
        this._isLoading.set(false);
        this._error.set(error.error?.message || 'Failed to start registration');
        return throwError(() => error);
      })
    );
  }

  /**
   * Complete the registration process by verifying the OTP and creating the account.
   * Step 2 of 2: Verifies email ownership, creates family and user, returns auth tokens.
   * @param request Verification request with session ID and OTP code
   * @returns Observable with created family and auth tokens
   */
  completeRegistration(request: RegisterCompleteRequest): Observable<RegisterCompleteResult> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.post<RegisterCompleteResult>('auth/register/complete', request).pipe(
      tap((result) => {
        if (result.success && result.auth) {
          this.handleAuthSuccess(result.auth);
        } else {
          this._isLoading.set(false);
        }
      }),
      catchError((error) => {
        this._isLoading.set(false);
        this._error.set(error.error?.message || 'Registration failed');
        return throwError(() => error);
      })
    );
  }

  // ============================================
  // Token Management
  // ============================================

  /**
   * Get current access token
   */
  getAccessToken(): string | null {
    return this.getStoredTokens()?.accessToken ?? null;
  }

  /**
   * Refresh the access token using refresh token
   */
  refreshToken(): Observable<TokenPair> {
    const tokens = this.getStoredTokens();
    if (!tokens?.refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.api.post<TokenPair>('auth/refresh', { refreshToken: tokens.refreshToken }).pipe(
      tap((newTokens) => {
        this.storeTokens(newTokens);
        const user = this.decodeToken(newTokens.accessToken);
        this._user.set(user);
        this.scheduleTokenRefresh(newTokens);
      }),
      catchError((error) => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  /**
   * Logout the current user
   */
  logout(): void {
    // Clear any scheduled token refresh
    if (this.refreshTokenTimeout) {
      clearTimeout(this.refreshTokenTimeout);
    }

    // Clear stored data
    this.storage.remove(environment.auth.tokenStorageKey);
    this.storage.remove(environment.auth.userStorageKey);

    // Reset state
    this._user.set(null);
    this._error.set(null);

    // Navigate to login
    this.router.navigate(['/auth/login']);
  }

  /**
   * Fetch the current user's full profile and update the user signal
   * This is useful after login to get profile data like color and avatar
   */
  loadUserProfile(): Observable<User | null> {
    const currentUser = this._user();
    if (!currentUser?.familyId || !currentUser?.id) {
      return of(null);
    }

    return this.api.get<User>(`users/family/${currentUser.familyId}/${currentUser.id}`).pipe(
      tap((fullUser) => {
        if (fullUser) {
          // Update the user signal with profile data
          this._user.update((user) => {
            if (user) {
              return {
                ...user,
                profile: fullUser.profile,
              };
            }
            return user;
          });
        }
      }),
      catchError((error) => {
        console.error('Failed to load user profile:', error);
        return of(null);
      })
    );
  }

  /**
   * Update the user profile in the current user signal
   */
  updateUserProfile(profile: UserProfile): void {
    this._user.update((user) => {
      if (user) {
        return {
          ...user,
          profile,
        };
      }
      return user;
    });
  }

  // ============================================
  // Private Helper Methods
  // ============================================

  private handleAuthSuccess(tokens: TokenPair): void {
    this._isLoading.set(false);
    this.storeTokens(tokens);

    const user = this.decodeToken(tokens.accessToken);
    this._user.set(user);

    this.scheduleTokenRefresh(tokens);
  }

  private storeTokens(tokens: TokenPair): void {
    this.storage.set(environment.auth.tokenStorageKey, tokens);
  }

  private getStoredTokens(): TokenPair | null {
    return this.storage.get<TokenPair>(environment.auth.tokenStorageKey);
  }

  private scheduleTokenRefresh(tokens: TokenPair): void {
    // Clear existing timeout
    if (this.refreshTokenTimeout) {
      clearTimeout(this.refreshTokenTimeout);
    }

    // Schedule refresh 1 minute before expiry
    const expiresInMs = (tokens.expiresIn - 60) * 1000;
    if (expiresInMs > 0) {
      this.refreshTokenTimeout = setTimeout(() => {
        this.refreshToken().subscribe();
      }, expiresInMs);
    }
  }

  private decodeToken(token: string): AuthUserInfo | null {
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));

      // JWT claims use different naming conventions:
      // - Standard claims: 'sub', 'email'
      // - .NET ClaimTypes become full URIs: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name
      // - Custom claims: 'display_name', 'family_id'
      const nameClaimUri = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
      const roleClaimUri = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

      // Extract profile data if included in token
      const profile = decoded.profile_color || decoded.profileColor
        ? {
            color: decoded.profile_color || decoded.profileColor,
            avatarUrl: decoded.profile_avatar_url || decoded.avatarUrl,
          }
        : undefined;

      return {
        id: decoded.sub,
        email: decoded.email,
        displayName: decoded.display_name || decoded[nameClaimUri] || decoded.name || decoded.displayName,
        familyId: decoded.family_id || decoded.familyId,
        role: decoded[roleClaimUri] || decoded.role,
        authMethod: decoded.auth_method || decoded.authMethod,
        mfaVerified: decoded.mfa_verified || decoded.mfaVerified || false,
        profile,
      };
    } catch {
      return null;
    }
  }

  private isTokenExpired(token: string): boolean {
    try {
      const payload = token.split('.')[1];
      const decoded = JSON.parse(atob(payload));
      const exp = decoded.exp * 1000; // Convert to milliseconds
      return Date.now() >= exp;
    } catch {
      return true;
    }
  }
}
