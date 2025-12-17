namespace BikeTracking.Domain.Results;

/// <summary>
/// Extension methods for working with Results functionally.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Requires a value to be non-null, or returns a validation failure.
    /// </summary>
    public static Result<T> RequireNotNull<T>(T? value, string paramName)
        where T : class
    {
        if (value is null)
            return new Result<T>.Failure(Error.ValidationFailed($"{paramName} cannot be null."));
        return new Result<T>.Success(value);
    }

    /// <summary>
    /// Requires a condition to be true, or returns a failure.
    /// </summary>
    public static Result<Unit> Require(bool condition, Error error) =>
        condition ? new Result<Unit>.Success(Unit.Value) : new Result<Unit>.Failure(error);

    /// <summary>
    /// Combines multiple Results, short-circuiting on first failure.
    /// </summary>
    public static Result<Unit> Combine(params Result<Unit>[] results)
    {
        ArgumentNullException.ThrowIfNull(results);
        foreach (var result in results)
        {
            if (result is Result<Unit>.Failure failure)
                return failure;
        }
        return new Result<Unit>.Success(Unit.Value);
    }

    /// <summary>
    /// Maps a sequence of results to a sequence of values or fails on first error.
    /// </summary>
    public static Result<IEnumerable<T>> Sequence<T>(IEnumerable<Result<T>> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        var values = new List<T>();
        foreach (var result in results)
        {
            if (result is Result<T>.Failure failure)
                return new Result<IEnumerable<T>>.Failure(failure.Error);
            if (result is Result<T>.Success success)
                values.Add(success.Value);
        }
        return new Result<IEnumerable<T>>.Success(values);
    }

    /// <summary>
    /// Converts a Result to an HTTP status code based on error severity.
    /// </summary>
    public static int ToStatusCode<T>(this Result<T> result) =>
        result switch
        {
            Result<T>.Success => 200,
            Result<T>.Failure failure => failure.Error.Severity switch
            {
                ErrorSeverity.Warning => 400,
                ErrorSeverity.Error => 500,
                ErrorSeverity.Critical => 503,
                _ => 500,
            },
            _ => 500,
        };

    /// <summary>
    /// Recovers from a failure by trying an alternative function.
    /// </summary>
    public static Result<T> Recover<T>(this Result<T> result, Func<Error, Result<T>> recover)
    {
        ArgumentNullException.ThrowIfNull(recover);
        return result switch
        {
            Result<T>.Success => result,
            Result<T>.Failure failure => recover(failure.Error),
            _ => result,
        };
    }

    /// <summary>
    /// Pattern matches on Result for async operations.
    /// </summary>
    public static async Task<TResult> Match<T, TResult>(
        this Result<T> result,
        Func<T, Task<TResult>> success,
        Func<Error, TResult> failure
    )
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        return result switch
        {
            Result<T>.Success s => await success(s.Value),
            Result<T>.Failure f => failure(f.Error),
            _ => throw new InvalidOperationException("Unknown Result type"),
        };
    }

    /// <summary>
    /// Pattern matches on Result for async operations with async failure handler.
    /// </summary>
    public static async Task<TResult> Match<T, TResult>(
        this Result<T> result,
        Func<T, Task<TResult>> success,
        Func<Error, Task<TResult>> failure
    )
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        return result switch
        {
            Result<T>.Success s => await success(s.Value),
            Result<T>.Failure f => await failure(f.Error),
            _ => throw new InvalidOperationException("Unknown Result type"),
        };
    }

    /// <summary>
    /// Pattern matches on Result for sync operations.
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> success,
        Func<Error, TResult> failure
    )
    {
        ArgumentNullException.ThrowIfNull(success);
        ArgumentNullException.ThrowIfNull(failure);
        return result switch
        {
            Result<T>.Success s => success(s.Value),
            Result<T>.Failure f => failure(f.Error),
            _ => throw new InvalidOperationException("Unknown Result type"),
        };
    }
}
