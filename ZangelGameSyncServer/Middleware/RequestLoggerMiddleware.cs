namespace ZangelGameSyncServer.Middleware
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggerMiddleware> _logger;

        public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("({host}) {method} {url}", context.Connection.RemoteIpAddress?.ToString(), context.Request.Method.ToUpper(), context.Request.Path);

            await _next(context);

            _logger.LogInformation("Outgoing response: {statusCode}", context.Response.StatusCode);
        }
    }
}
