namespace HotelListing.Api.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string Error { get; }
    public ErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty, ErrorType.None);

    public static Result<T> BadRequest(string error) => new(false, default, error, ErrorType.BadRequest);
    public static Result<T> NotFound(string error) => new(false, default, error, ErrorType.NotFound);
    public static Result<T> Conflict(string error) => new(false, default, error, ErrorType.Conflict);
    public static Result<T> Failure(string error) => new(false, default, error, ErrorType.Failure);
}

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public ErrorType ErrorType { get; }

    private Result(bool isSuccess, string error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, string.Empty, ErrorType.None);

    public static Result BadRequest(string error) => new(false, error, ErrorType.BadRequest);
    public static Result NotFound(string error) => new(false, error, ErrorType.NotFound);
    public static Result Conflict(string error) => new(false, error, ErrorType.Conflict);
    public static Result Failure(string error) => new(false, error, ErrorType.Failure);
}
