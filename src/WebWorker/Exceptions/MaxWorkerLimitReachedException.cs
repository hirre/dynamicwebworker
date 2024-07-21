namespace WebWorker.Exceptions
{
    public class MaxWorkerLimitReachedException(string s) : HttpResponseException(StatusCodes.Status429TooManyRequests, s, null)
    {
    }
}
