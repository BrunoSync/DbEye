using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Common.Options;
using DbEye.Core.Collector;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DbEye.Core.Middleware
{
    public class DbEyeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DbEyeMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public DbEyeMiddleware(RequestDelegate next, ILogger<DbEyeMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<DbEyeOptions> options)
        {
            var currentEnvironment = _env.EnvironmentName;

            if (!options.Value.AllowedEnvironments.Contains(currentEnvironment, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"DbEye is not allowed in the '{currentEnvironment}' environment. " +
                    $"Allowed environments: {string.Join(", ", options.Value.AllowedEnvironments)}. " +
                    "Remove AddDbEye() from your configuration or update AllowedEnvironments."
                );
            }

            // Path
            var path = context.Request.Path.ToString();

            // Exclude endpoints
            var excludeEndpoints = options.Value.GetExcludedEndpoints();

            if (excludeEndpoints.Any(e => path.StartsWith(e)))
            {
                await _next(context);
                return;
            }

            await _next(context);

            // N+1 and Slow Queries
            var collector = context.RequestServices.GetRequiredService<DbEyeCollector>();

            var NPlus1Threshold = options.Value.EndpointNPlus1Thresholds.TryGetValue(path, out var queryLimit)
                ? queryLimit : options.Value.NPlus1Threshold;

            var sqlQueries = collector.Queries  
                                .GroupBy(s => s.Sql)
                                .Where(s => s.Count() > NPlus1Threshold).ToList();

            var ms = options.Value.EndpointThresholds.TryGetValue(path, out var custom)
                ? custom : options.Value.SlowQueryThresholdMs;

            var thresHoldMs = TimeSpan.FromMilliseconds(ms);

            var slowQueries = collector.Queries
                                .Where(d => d.DurationInMs >= thresHoldMs).ToList();
                            
            var separator = new string('-', 50);

            foreach (var item in sqlQueries)
            {
                _logger.LogWarning($"\n{separator}\n\n⚠️  N+1 detected at {context.Request.Method} {context.Request.Path}\nQuery repeated {item.Count()}x - {item.Key} \n\n{separator}");
            }

            foreach (var item in slowQueries)
            {
                _logger.LogWarning($"\n{separator}\n\n⚠️  Slow query detected at {context.Request.Method} {context.Request.Path}\nDuration: {(int)item.DurationInMs.TotalMilliseconds}ms - {item.Sql} \n\n{separator}");
            }

            if (!sqlQueries.Any() && !slowQueries.Any())
                _logger.LogInformation($"\n{separator}\n\n✅  {context.Request.Method} {context.Request.Path} - no issues detected \n\n{separator}");
        }
    }
}