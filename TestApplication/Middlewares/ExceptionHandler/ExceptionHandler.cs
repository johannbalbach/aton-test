using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using TestApplication.DataAccessLayer.Exceptions;

namespace TestApplication.Middlewares.ExceptionHandler
{
    public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
    {
        ILogger<ExceptionHandler> _logger = logger;
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            _logger.LogError(
                exception,
                $"Error occured. TraceId : {traceId}",
                Environment.MachineName,
                traceId
                ); // optional

            var (statusCode, title) = MapException(exception);

            await Results.Problem(
                title: title,
                statusCode: statusCode,
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = traceId,
                    ["machine"] = Environment.MachineName,
                    ["data"] = exception.Data
                }
            ).ExecuteAsync(httpContext);

            return true;
        }


        private static (int StatusCode, string Title) MapException(Exception exception)
        {
            return exception switch
            {
                InvalidLoginException invalidLoginException => (StatusCodes.Status403Forbidden, invalidLoginException.Message),
                ForbiddenException forbiddenException => (StatusCodes.Status403Forbidden, forbiddenException.Message),
                NotFoundException notFoundException => (StatusCodes.Status404NotFound, notFoundException.Message),
                HttpRequestException httpRequestException => (StatusCodes.Status503ServiceUnavailable, httpRequestException.Message),
                BadRequestException badRequestException => (StatusCodes.Status400BadRequest, badRequestException.Message),
                _ => (StatusCodes.Status500InternalServerError, "Some stupid error occured")
            };
        }
    }
}
