import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

/**
 * API error response structure
 */
export interface ApiError {
  message: string;
  code?: string;
  details?: Record<string, string[]>;
}

/**
 * HTTP interceptor for global error handling.
 * Transforms HTTP errors into a consistent format.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let apiError: ApiError;

      if (error.error instanceof ErrorEvent) {
        // Client-side error
        apiError = {
          message: 'A network error occurred. Please check your connection.',
          code: 'NETWORK_ERROR',
        };
      } else {
        // Server-side error
        switch (error.status) {
          case 0:
            apiError = {
              message: 'Unable to reach the server. Please try again later.',
              code: 'CONNECTION_ERROR',
            };
            break;
          case 400:
            apiError = {
              message: error.error?.message || 'Invalid request. Please check your input.',
              code: 'BAD_REQUEST',
              details: error.error?.errors,
            };
            break;
          case 403:
            apiError = {
              message: error.error?.message || 'You do not have permission to perform this action.',
              code: 'FORBIDDEN',
            };
            break;
          case 404:
            apiError = {
              message: error.error?.message || 'The requested resource was not found.',
              code: 'NOT_FOUND',
            };
            break;
          case 409:
            apiError = {
              message: error.error?.message || 'A conflict occurred with the current state.',
              code: 'CONFLICT',
            };
            break;
          case 422:
            apiError = {
              message: error.error?.message || 'Validation failed.',
              code: 'VALIDATION_ERROR',
              details: error.error?.errors,
            };
            break;
          case 429:
            apiError = {
              message: 'Too many requests. Please wait a moment and try again.',
              code: 'RATE_LIMITED',
            };
            break;
          case 500:
          case 502:
          case 503:
          case 504:
            apiError = {
              message: 'A server error occurred. Please try again later.',
              code: 'SERVER_ERROR',
            };
            break;
          default:
            apiError = {
              message: error.error?.message || 'An unexpected error occurred.',
              code: 'UNKNOWN_ERROR',
            };
        }
      }

      // Log error in development
      if (!error.url?.includes('/auth/')) {
        console.error('API Error:', {
          url: error.url,
          status: error.status,
          error: apiError,
        });
      }

      return throwError(() => ({ ...error, error: apiError }));
    })
  );
};
