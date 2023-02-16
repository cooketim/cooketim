using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ResultsDataService.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly ILogger<ApiKeyMiddleware> logger;

        private readonly RequestDelegate next;
        private const string APIKEY = "XApiKey";
        public ApiKeyMiddleware(ILogger<ApiKeyMiddleware> logger, RequestDelegate next)
        {
            this.logger = logger;
            this.next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.TryGetValue(APIKEY, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                logger.LogError("Request '{0}', Api Key was not provided", context.Request.Path);
                await context.Response.WriteAsync("Api Key was not provided");
                return;
            }
            var appSettings = context.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<string>(APIKEY);
            if (!apiKey.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                logger.LogError("Request '{0}', Unauthorized client", context.Request.Path);
                await context.Response.WriteAsync("Unauthorized client");
                return;
            }
            await next(context);
        }
    }
}
