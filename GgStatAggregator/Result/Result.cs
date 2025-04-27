using System;

namespace GgStatAggregator.Result
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Message { get; }

        protected Result(bool isSuccess, string message = null)
        {
            if (!isSuccess && string.IsNullOrEmpty(message))
                throw new InvalidOperationException("Failure result must have a message.");

            IsSuccess = isSuccess;
            Message = message;
        }

        public static Result Success(string message = null) => new(true, message);
        public static Result Failure(string message) => new(false, message);
    }

    public class Result<T> : Result
    {
        public T Value { get; }

        protected internal Result(T value, bool isSuccess, string error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static Result<T> Success(T value, string message = null) => new(value, true, message);
        public static new Result<T> Failure(string message) => new(default, false, message);
    }
}
