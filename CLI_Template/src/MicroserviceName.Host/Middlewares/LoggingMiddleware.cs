using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MicroserviceName.Host.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var state = new Dictionary<string, object>();

            var headers = context.Request.Headers;

            if (headers.TryGetValue("ConversationId", out var conversationId))
            {
                state.Add("conversationid", conversationId);
            }

            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (remoteIp != null)
            {
                state.Add("clientip", remoteIp);
            }

            using (_logger.BeginScope(state))
            {
                await _next(context);
            }
        }
    }
}
