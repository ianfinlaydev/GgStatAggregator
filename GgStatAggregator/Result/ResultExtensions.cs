namespace GgStatAggregator.Result
{
    public static class ResultExtensions
    {
        public static async Task<Result<T>> OnFailureAsync<T>(this Task<Result<T>> resultTask, Func<Task<Result<T>>> func)
        {
            var result = await resultTask;
            return result.IsSuccess ? result : await func();
        }

        public static async Task<Result<U>> MapAsync<T, U>(this Task<Result<T>> resultTask, Func<T, U> func)
        {
            var result = await resultTask;
            if (result.IsFailure)
                return Result<U>.Failure(result.Message!);

            return Result<U>.Success(func(result.Value!));
        }
    }
}
