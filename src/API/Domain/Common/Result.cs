namespace Domain.Common;

public enum ErrorType
{
    None = 0,
    Failure = 1,
    Unexpected = 2,
    Validation = 3,
    Conflict = 4,
    NotFound = 5,
    Unauthorized = 6
}

public record Error(string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, ErrorType.None);

    public static Error Failure(string message) => new(message, ErrorType.Failure);
    public static Error Unexpected(string message) => new(message, ErrorType.Unexpected);
    public static Error Validation(string message) => new(message, ErrorType.Validation);
    public static Error Conflict(string message) => new(message, ErrorType.Conflict);
    public static Error NotFound(string message) => new(message, ErrorType.NotFound);
    public static Error Unauthorized(string message) => new(message, ErrorType.Unauthorized);
}

public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A success result cannot have an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result Failure(string message, ErrorType type = ErrorType.Failure) => Failure(new Error(message, type));
    
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess() : onFailure(Error);
    }
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    protected Result(TValue? value, bool isSuccess, Error error) 
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<TValue> Success(TValue value) => new(value, true, Error.None);
    public static new Result<TValue> Failure(Error error) => new(default, false, error);
    public static new Result<TValue> Failure(string message, ErrorType type = ErrorType.Failure) => Failure(new Error(message, type));

    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(Error);
    }
}