using System.Net;
using System.Text.Json;
using ChatService.Api.Models;

namespace ChatService.Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An unhandled exception occurred: {ExceptionMessage}", exception.Message);
                await HandleExceptionAsync(context, exception);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Message = "An error occurred while processing your request",
                StatusCode = (int)HttpStatusCode.InternalServerError,
            };

            switch (exception)
            {
                case ArgumentException argEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = argEx.Message;
                    break;
                case InvalidOperationException invEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = invEx.Message;
                    break;
                default:
                    response.Details = exception.Message;
                    break;
            }

            context.Response.StatusCode = response.StatusCode;
            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
