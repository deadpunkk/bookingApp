namespace BookingApi.Common;

public class Result<T>
{ 
    private Result(T? value, ErrorCode error, bool isSuccess)
    {
        Value = value;
        Error = error;
        IsSuccess = isSuccess;
    }
    
    public T? Value { get; } 
    public ErrorCode Error { get; }
    public bool IsSuccess { get; }

    public static Result<T> Success(T value) => new Result<T>(value, ErrorCode.None, isSuccess: true);
    public static Result<T> Failure(ErrorCode error) => new Result<T>(default, error,  false);
}