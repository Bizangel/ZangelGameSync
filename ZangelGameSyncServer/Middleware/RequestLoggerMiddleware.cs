namespace ZangelGameSyncServer.Middleware
{
    public class RequestLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggerMiddleware> _logger;

        private readonly string requestSeparator = " " + (new string('=', 10)) + " ";
        public RequestLoggerMiddleware(RequestDelegate next, ILogger<RequestLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("({host}) {method} {url}" + requestSeparator, context.Connection.RemoteIpAddress?.ToString(), context.Request.Method.ToUpper(), context.Request.Path);

            await _next(context);

            _logger.LogInformation("Outgoing response: {statusCode}" + requestSeparator, context.Response.StatusCode);
        }
    }
}
