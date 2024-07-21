namespace WebWorker.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? ErrorMessage { get; }
        public ErrorCodes ErrorCode { get; }
        public string? ErrorMessageDetail => $"{ErrorMessage} (error code: {(int)ErrorCode})";

        protected Result(bool isSuccess, T value, string? error, ErrorCodes errorCodes)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = error;
            ErrorCode = errorCodes;
        }

        protected Result(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        public static Result<T?> Success(T? value) => new(true, value, null, ErrorCodes.NoError);
        public static Result<T?> Success() => new(true);
        public static Result<T?> Failure(string error, ErrorCodes errorCode) => new(false, default, error, errorCode);
    }
}
