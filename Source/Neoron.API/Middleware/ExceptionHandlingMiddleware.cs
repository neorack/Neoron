using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Neoron.API.Middleware
{
    /// <summary>
    /// Middleware for handling exceptions globally.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionHandlingMiddleware> logger;

        private static readonly Action<ILogger, Exception> LogUnhandledException =
            LoggerMessage.Define(
                LogLevel.Error,
                new EventId(1, nameof(InvokeAsync)),
                "An unhandled exception occurred");

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger instance.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogUnhandledException(logger, ex);
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "The resource was modified by another user."),
                DbUpdateException => (HttpStatusCode.BadRequest, "Unable to save changes to the database."),
                KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred."),
            };

            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsJsonAsync(new { error = message }).ConfigureAwait(false);
        }
    }
}
