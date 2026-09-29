namespace SkillSwap.Application.Common.Models;

/// <summary>
/// Functional result envelope for enterprise command/query responses.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors?.ToArray() ?? (error != null ? new[] { error } : Array.Empty<string>());
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string[] Errors { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) => new(false, null, errors);
}

/// <summary>
/// Generic result envelope containing typed data payload.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    protected Result(T? value, bool isSuccess, string? error, IEnumerable<string>? errors = null)
        : base(isSuccess, error, errors)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static Result<T> Success(T value) => new(value, true, null);
    public new static Result<T> Failure(string error) => new(default, false, error);
    public new static Result<T> Failure(IEnumerable<string> errors) => new(default, false, null, errors);
}
