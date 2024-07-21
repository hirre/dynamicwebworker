namespace WebWorker.Exceptions
{
    public class DuplicateWorkerException(string s) : HttpResponseException(StatusCodes.Status409Conflict, s, null)
    {
    }
}
