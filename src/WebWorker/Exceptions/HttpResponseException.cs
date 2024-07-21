namespace WebWorker.Exceptions
{
    public class HttpResponseException : Exception
    {
        public HttpResponseException(int statusCode, string msg, object? value = null) : base(msg)
        {
            StatusCode = statusCode;
            Value = value;
        }

        public int StatusCode { get; }

        public object? Value { get; }
    }
}
