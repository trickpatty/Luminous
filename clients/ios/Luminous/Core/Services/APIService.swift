//
//  APIService.swift
//  Luminous
//
//  Created by Luminous Team
//  Copyright © 2026 Luminous. All rights reserved.
//

import Foundation

/// Service for making API requests to the Luminous backend.
///
/// Handles authentication headers, request retries, and error mapping.
final class APIService {
    // MARK: - Singleton

    static let shared = APIService()

    // MARK: - Properties

    private let session: URLSession
    private let baseURL: URL
    private let decoder: JSONDecoder
    private let encoder: JSONEncoder

    /// Current access token for authenticated requests.
    private var accessToken: String?

    /// Maximum number of retry attempts for failed requests.
    private let maxRetries = 3

    /// Base delay for exponential backoff (in seconds).
    private let baseRetryDelay: TimeInterval = 1.0

    // MARK: - Initialization

    private init() {
        let configuration = URLSessionConfiguration.default
        configuration.timeoutIntervalForRequest = AppConfiguration.apiTimeout
        configuration.timeoutIntervalForResource = AppConfiguration.apiTimeout * 2
        configuration.waitsForConnectivity = true

        self.session = URLSession(configuration: configuration)
        self.baseURL = AppConfiguration.apiBaseURL
        self.decoder = JSONDecoder()
        self.encoder = JSONEncoder()

        // Configure JSON coding
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        decoder.dateDecodingStrategy = .iso8601
        encoder.keyEncodingStrategy = .convertToSnakeCase
        encoder.dateEncodingStrategy = .iso8601
    }

    // MARK: - Public Methods

    /// Set the access token for authenticated requests.
    func setAccessToken(_ token: String?) {
        accessToken = token
    }

    /// Perform a GET request.
    func get<T: Decodable>(_ path: String, queryItems: [URLQueryItem]? = nil) async throws -> T {
        let request = try buildRequest(method: "GET", path: path, queryItems: queryItems)
        return try await execute(request)
    }

    /// Perform a POST request with a JSON body.
    func post<T: Decodable, Body: Encodable>(_ path: String, body: Body) async throws -> T {
        var request = try buildRequest(method: "POST", path: path)
        request.httpBody = try encoder.encode(body)
        return try await execute(request)
    }

    /// Perform a POST request without a body.
    func post<T: Decodable>(_ path: String) async throws -> T {
        let request = try buildRequest(method: "POST", path: path)
        return try await execute(request)
    }

    /// Perform a PUT request with a JSON body.
    func put<T: Decodable, Body: Encodable>(_ path: String, body: Body) async throws -> T {
        var request = try buildRequest(method: "PUT", path: path)
        request.httpBody = try encoder.encode(body)
        return try await execute(request)
    }

    /// Perform a DELETE request.
    func delete<T: Decodable>(_ path: String) async throws -> T {
        let request = try buildRequest(method: "DELETE", path: path)
        return try await execute(request)
    }

    /// Perform a DELETE request without expecting a response body.
    func delete(_ path: String) async throws {
        let request = try buildRequest(method: "DELETE", path: path)
        let _: EmptyResponse = try await execute(request)
    }

    // MARK: - Private Methods

    private func buildRequest(
        method: String,
        path: String,
        queryItems: [URLQueryItem]? = nil
    ) throws -> URLRequest {
        var urlComponents = URLComponents(url: baseURL.appendingPathComponent(path), resolvingAgainstBaseURL: true)
        urlComponents?.queryItems = queryItems

        guard let url = urlComponents?.url else {
            throw APIError.invalidURL
        }

        var request = URLRequest(url: url)
        request.httpMethod = method
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        request.setValue("application/json", forHTTPHeaderField: "Accept")
        request.setValue("Luminous-iOS/\(AppConfiguration.appVersion)", forHTTPHeaderField: "User-Agent")

        if let accessToken {
            request.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
        }

        return request
    }

    private func execute<T: Decodable>(_ request: URLRequest, retryCount: Int = 0) async throws -> T {
        do {
            let (data, response) = try await session.data(for: request)

            guard let httpResponse = response as? HTTPURLResponse else {
                throw APIError.invalidResponse
            }

            // Log in debug mode
            if AppConfiguration.isLoggingEnabled {
                print("[\(request.httpMethod ?? "?")] \(request.url?.path ?? "") -> \(httpResponse.statusCode)")
            }

            switch httpResponse.statusCode {
            case 200...299:
                return try decoder.decode(T.self, from: data)

            case 401:
                throw APIError.unauthorized

            case 403:
                throw APIError.forbidden

            case 404:
                throw APIError.notFound

            case 422:
                let validationError = try? decoder.decode(ValidationErrorResponse.self, from: data)
                throw APIError.validationError(validationError?.errors ?? [:])

            case 429:
                if retryCount < maxRetries {
                    let delay = baseRetryDelay * pow(2.0, Double(retryCount))
                    try await Task.sleep(nanoseconds: UInt64(delay * 1_000_000_000))
                    return try await execute(request, retryCount: retryCount + 1)
                }
                throw APIError.rateLimited

            case 500...599:
                if retryCount < maxRetries {
                    let delay = baseRetryDelay * pow(2.0, Double(retryCount))
                    try await Task.sleep(nanoseconds: UInt64(delay * 1_000_000_000))
                    return try await execute(request, retryCount: retryCount + 1)
                }
                throw APIError.serverError(httpResponse.statusCode)

            default:
                throw APIError.httpError(httpResponse.statusCode)
            }

        } catch let error as APIError {
            throw error
        } catch is DecodingError {
            throw APIError.decodingError
        } catch {
            // Network error - retry with backoff
            if retryCount < maxRetries {
                let delay = baseRetryDelay * pow(2.0, Double(retryCount))
                try await Task.sleep(nanoseconds: UInt64(delay * 1_000_000_000))
                return try await execute(request, retryCount: retryCount + 1)
            }
            throw APIError.networkError(error)
        }
    }
}

// MARK: - API Error

/// Errors that can occur when making API requests.
enum APIError: LocalizedError {
    case invalidURL
    case invalidResponse
    case unauthorized
    case forbidden
    case notFound
    case validationError([String: [String]])
    case rateLimited
    case serverError(Int)
    case httpError(Int)
    case decodingError
    case networkError(Error)

    var errorDescription: String? {
        switch self {
        case .invalidURL:
            return "Invalid URL"
        case .invalidResponse:
            return "Invalid response from server"
        case .unauthorized:
            return "Authentication required"
        case .forbidden:
            return "Access denied"
        case .notFound:
            return "Resource not found"
        case .validationError(let errors):
            return errors.values.flatMap { $0 }.joined(separator: ", ")
        case .rateLimited:
            return "Too many requests. Please try again later."
        case .serverError(let code):
            return "Server error (\(code))"
        case .httpError(let code):
            return "HTTP error (\(code))"
        case .decodingError:
            return "Failed to process server response"
        case .networkError:
            return "Network connection error"
        }
    }
}

// MARK: - Response Types

/// Empty response for requests that don't return data.
struct EmptyResponse: Decodable {}

/// Validation error response from the API.
struct ValidationErrorResponse: Decodable {
    let errors: [String: [String]]
}

/// Standard API response wrapper.
struct APIResponse<T: Decodable>: Decodable {
    let data: T
    let success: Bool
    let message: String?
}
