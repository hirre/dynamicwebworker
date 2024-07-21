using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebWorker.Exceptions
{
    public class ExceptionsHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionsHandler> _logger;

        public ExceptionsHandler(ILogger<ExceptionsHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ProblemDetails result = new ProblemDetails();

            switch (exception)
            {
                case HttpResponseException httpEx:
                    result = new ProblemDetails
                    {
                        Status = httpEx.StatusCode,
                        Type = httpEx.GetType().Name,
                        Title = httpEx.Message,
                        Detail = httpEx.Message,
                        Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                    };

                    httpContext.Response.StatusCode = httpEx.StatusCode;

                    _logger.LogError(httpEx, $"Exception occured : {httpEx.Message}");

                    break;

                default:

                    result = new ProblemDetails
                    {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Type = exception.GetType().Name,
                        Title = "An unexpected error occurred",
                        Detail = exception.Message,
                        Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
                    };

                    _logger.LogError(exception, $"Exception occured : {exception.Message}");

                    break;
            }

            await httpContext.Response.WriteAsJsonAsync(result, cancellationToken: cancellationToken);

            return true;
        }
    }
}
