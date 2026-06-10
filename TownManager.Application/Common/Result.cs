namespace TownManager.Application.Common;

public class Result
{
    public bool Succeeded { get; protected set; }
    public IEnumerable<string> Errors { get; protected set; } = [];

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors };
}

public class Result<T> : Result
{
    public T? Value { get; private init; }

    public static Result<T> Success(T value) => new() { Succeeded = true, Value = value };
    public new static Result<T> Failure(IEnumerable<string> errors) => new() { Succeeded = false, Errors = errors };
}