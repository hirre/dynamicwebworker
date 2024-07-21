namespace WebWorker.Exceptions
{
    public class WorkerNotFoundException(string msg, object? value = null) : HttpResponseException(StatusCodes.Status404NotFound, msg, value)
    {
    }
}
