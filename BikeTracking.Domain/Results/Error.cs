namespace BikeTracking.Domain.Results;

/// <summary>
/// Represents an error with a code, message, and severity level.
/// Domain-friendly alternative to exception types.
/// </summary>
public record Error(string Code, string Message, ErrorSeverity Severity = ErrorSeverity.Error)
{
    /// <summary>
    /// Creates a validation error (warning severity).
    /// </summary>
    public static Error ValidationFailed(string message) =>
        new("VALIDATION_FAILED", message, ErrorSeverity.Warning);

    /// <summary>
    /// Creates a not-found error (warning severity).
    /// </summary>
    public static Error NotFound(string message) =>
        new("NOT_FOUND", message, ErrorSeverity.Warning);

    /// <summary>
    /// Creates a conflict error (warning severity).
    /// </summary>
    public static Error Conflict(string message) =>
        new("CONFLICT", message, ErrorSeverity.Warning);

    /// <summary>
    /// Creates an unexpected/internal error.
    /// </summary>
    public static Error Unexpected(string message) =>
        new("UNEXPECTED", message, ErrorSeverity.Error);

    /// <summary>
    /// Creates a critical error (service unavailable).
    /// </summary>
    public static Error Critical(string message) =>
        new("CRITICAL", message, ErrorSeverity.Critical);

    /// <summary>
    /// Creates an unauthorized error.
    /// </summary>
    public static Error Unauthorized(string message) =>
        new("UNAUTHORIZED", message, ErrorSeverity.Warning);

    /// <summary>
    /// Creates a forbidden error.
    /// </summary>
    public static Error Forbidden(string message) =>
        new("FORBIDDEN", message, ErrorSeverity.Warning);
}

/// <summary>
/// Severity levels for errors, used for HTTP status code mapping.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>
    /// Expected validation/business logic error. Maps to 4xx.
    /// </summary>
    Warning = 0,

    /// <summary>
    /// Unexpected/internal error. Maps to 500.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Critical/system error. Maps to 503.
    /// </summary>
    Critical = 2
}
