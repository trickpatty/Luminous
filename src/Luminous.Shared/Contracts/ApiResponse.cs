namespace Luminous.Shared.Contracts;

/// <summary>
/// Standard API response wrapper.
/// </summary>
/// <typeparam name="T">The data type.</typeparam>
public sealed record ApiResponse<T>
{
    /// <summary>
    /// Gets or sets whether the request was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Gets or sets error details if the request failed.
    /// </summary>
    public ErrorDetails? Error { get; init; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data
    };

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    public static ApiResponse<T> Fail(string code, string message, IDictionary<string, string[]>? errors = null) => new()
    {
        Success = false,
        Error = new ErrorDetails
        {
            Code = code,
            Message = message,
            Errors = errors
        }
    };
}

/// <summary>
/// Error details for failed responses.
/// </summary>
public sealed record ErrorDetails
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets validation errors by field.
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; init; }
}

/// <summary>
/// Paginated response wrapper.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed record PaginatedResponse<T>
{
    /// <summary>
    /// Gets or sets the items in this page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets or sets the total number of items.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Gets whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
