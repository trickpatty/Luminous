import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, throwError, of } from 'rxjs';
import { map, tap, catchError, switchMap } from 'rxjs/operators';

import { ApiService } from './api.service';
import { StorageService } from './storage.service';
import { environment } from '../../../environments/environment';
import {
  TokenPair,
  AuthState,
  AuthUserInfo,
  LoginRequest,
  VerifyOtpRequest,
  RegisterStartRequest,
  RegisterStartResult,
  RegisterCompleteRequest,
  RegisterCompleteResult,
  PasskeyRegistrationOptions,
  PasskeyAuthenticationOptions,
  PasskeyRegistrationResponse,
  PasskeyAuthenticationResponse,
  PasskeyCredential,
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
  verifyOtp(email: string, code: string): Observable<TokenPair> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.post<TokenPair>('auth/otp/verify', { email, code } as VerifyOtpRequest).pipe(
      tap((tokens) => this.handleAuthSuccess(tokens)),
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
   */
  startPasskeyRegistration(email: string): Observable<PasskeyRegistrationOptions> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.api.post<PasskeyRegistrationOptions>('auth/passkey/register/start', { email }).pipe(
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
   * @param response Passkey credential response from browser
   */
  completePasskeyRegistration(response: PasskeyRegistrationResponse): Observable<TokenPair> {
    this._isLoading.set(true);

    return this.api.post<TokenPair>('auth/passkey/register/complete', response).pipe(
      tap((tokens) => this.handleAuthSuccess(tokens)),
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
   */
  startPasskeyAuthentication(email?: string): Observable<PasskeyAuthenticationOptions> {
    this._isLoading.set(true);
    this._error.set(null);

    const body = email ? { email } : {};
    return this.api.post<PasskeyAuthenticationOptions>('auth/passkey/authenticate/start', body).pipe(
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
   * @param response Passkey authentication response from browser
   */
  completePasskeyAuthentication(response: PasskeyAuthenticationResponse): Observable<TokenPair> {
    this._isLoading.set(true);

    return this.api.post<TokenPair>('auth/passkey/authenticate/complete', response).pipe(
      tap((tokens) => this.handleAuthSuccess(tokens)),
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
    return this.api.get<PasskeyCredential[]>('auth/passkey/list');
  }

  /**
   * Delete a passkey
   * @param credentialId Passkey credential ID
   */
  deletePasskey(credentialId: string): Observable<void> {
    return this.api.delete<void>(`auth/passkey/${credentialId}`);
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

      return {
        id: decoded.sub,
        email: decoded.email,
        displayName: decoded.name || decoded.displayName,
        familyId: decoded.family_id || decoded.familyId,
        role: decoded.role,
        authMethod: decoded.auth_method || decoded.authMethod,
        mfaVerified: decoded.mfa_verified || decoded.mfaVerified || false,
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
