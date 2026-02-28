namespace Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public bool IsFailure => !IsSuccess;
    public int StatusCode { get; }

    protected Result(bool isSuccess, string error, int statusCode)
    {
        if (isSuccess && error != string.Empty)
            throw new InvalidOperationException("A success result cannot have an error message.");
        if (!isSuccess && error == string.Empty)
            throw new InvalidOperationException("A failure result must have an error message.");

        IsSuccess = isSuccess;
        Error = error;
        StatusCode = statusCode;
    }

    public static Result Success(int statusCode = 200) => new(true, string.Empty, statusCode);
    public static Result Failure(string error, int statusCode = 400) => new(false, error, statusCode);
}

public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    protected Result(T? value, bool isSuccess, string error, int statusCode) 
        : base(isSuccess, error, statusCode)
    {
        _value = value;
    }

    public static Result<T> Success(T value, int statusCode = 200) => new(value, true, string.Empty, statusCode);
    public static new Result<T> Failure(string error, int statusCode = 400) => new(default, false, error, statusCode);
}
