import { HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs';

/**
 * Backend API response wrapper structure.
 */
interface ApiResponse<T> {
  success: boolean;
  data: T;
  error?: {
    code: string;
    message: string;
    errors?: Record<string, string[]>;
  };
}

/**
 * Type guard to check if a response is an ApiResponse wrapper.
 */
function isApiResponse<T>(body: unknown): body is ApiResponse<T> {
  return (
    typeof body === 'object' &&
    body !== null &&
    'success' in body &&
    typeof (body as ApiResponse<T>).success === 'boolean' &&
    'data' in body
  );
}

/**
 * HTTP interceptor that unwraps ApiResponse from backend responses.
 *
 * The backend wraps all responses in { success: boolean, data: T, error?: {...} }.
 * This interceptor extracts the `data` field for successful responses,
 * allowing services to work with the actual data types directly.
 */
export const apiResponseInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    map((event) => {
      if (event instanceof HttpResponse && event.body) {
        // Check if the response body is wrapped in ApiResponse
        if (isApiResponse(event.body)) {
          // Extract the data field for successful responses
          if (event.body.success) {
            return event.clone({ body: event.body.data });
          }
        }
      }
      return event;
    })
  );
};
