import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * HTTP request options
 */
export interface RequestOptions {
  headers?: HttpHeaders | { [header: string]: string | string[] };
  params?: HttpParams | { [param: string]: string | string[] };
  withCredentials?: boolean;
}

/**
 * Base API service for making HTTP requests to the backend.
 * Provides typed methods for common HTTP operations.
 */
@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  /**
   * Perform a GET request
   * @param endpoint API endpoint (relative to base URL)
   * @param options Request options
   * @returns Observable of response
   */
  get<T>(endpoint: string, options?: RequestOptions): Observable<T> {
    return this.http.get<T>(this.url(endpoint), options);
  }

  /**
   * Perform a POST request
   * @param endpoint API endpoint (relative to base URL)
   * @param body Request body
   * @param options Request options
   * @returns Observable of response
   */
  post<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.http.post<T>(this.url(endpoint), body, options);
  }

  /**
   * Perform a PUT request
   * @param endpoint API endpoint (relative to base URL)
   * @param body Request body
   * @param options Request options
   * @returns Observable of response
   */
  put<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.http.put<T>(this.url(endpoint), body, options);
  }

  /**
   * Perform a PATCH request
   * @param endpoint API endpoint (relative to base URL)
   * @param body Request body
   * @param options Request options
   * @returns Observable of response
   */
  patch<T>(endpoint: string, body: unknown, options?: RequestOptions): Observable<T> {
    return this.http.patch<T>(this.url(endpoint), body, options);
  }

  /**
   * Perform a DELETE request
   * @param endpoint API endpoint (relative to base URL)
   * @param options Request options
   * @returns Observable of response
   */
  delete<T>(endpoint: string, options?: RequestOptions): Observable<T> {
    return this.http.delete<T>(this.url(endpoint), options);
  }

  /**
   * Construct full URL from endpoint
   * @param endpoint API endpoint
   * @returns Full URL
   */
  private url(endpoint: string): string {
    // Remove leading slash if present
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
    return `${this.baseUrl}/${cleanEndpoint}`;
  }
}
