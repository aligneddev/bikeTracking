namespace BikeTracking.Domain.Results;

/// <summary>
/// Represents the outcome of an operation as a discriminated union.
/// Success contains a value; Failure contains error details.
/// Enables functional error handling without exceptions.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
public abstract record Result<T>
{
    /// <summary>
    /// Represents a successful operation with a value.
    /// </summary>
    public sealed record Success(T Value) : Result<T>;

    /// <summary>
    /// Represents a failed operation with error details.
    /// </summary>
    public sealed record Failure(Error Error) : Result<T>;

    /// <summary>
    /// Applies a function to the success value, or returns the failure.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> f)
    {
        ArgumentNullException.ThrowIfNull(f);
        return this switch
        {
            Success success => new Result<TNew>.Success(f(success.Value)),
            Failure failure => new Result<TNew>.Failure(failure.Error),
            _ => throw new InvalidOperationException("Unknown Result type")
        };
    }

    /// <summary>
    /// Chains result-returning functions (monadic bind).
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> f)
    {
        ArgumentNullException.ThrowIfNull(f);
        return this switch
        {
            Success success => f(success.Value),
            Failure failure => new Result<TNew>.Failure(failure.Error),
            _ => throw new InvalidOperationException("Unknown Result type")
        };
    }

    /// <summary>
    /// Applies a side effect to the success value.
    /// </summary>
    public Result<T> Tap(Action<T> f)
    {
        ArgumentNullException.ThrowIfNull(f);
        if (this is Success success)
            f(success.Value);
        return this;
    }

    /// <summary>
    /// Applies a side effect to the failure.
    /// </summary>
    public Result<T> TapFailure(Action<Error> f)
    {
        ArgumentNullException.ThrowIfNull(f);
        if (this is Failure failure)
            f(failure.Error);
        return this;
    }

    /// <summary>
    /// Recovers from failure with a default value.
    /// </summary>
    public T GetValueOrDefault(T defaultValue) =>
        this switch
        {
            Success success => success.Value,
            _ => defaultValue
        };

    /// <summary>
    /// Gets the error or null if successful.
    /// </summary>
    public Error? GetErrorOrNull() =>
        this switch
        {
            Failure failure => failure.Error,
            _ => null
        };
}

/// <summary>
/// Represents the absence of a value (unit type).
/// Used for operations that don't return a meaningful result.
/// </summary>
public class Unit
{
    public static readonly Unit Value = new();

    private Unit() { }
}
