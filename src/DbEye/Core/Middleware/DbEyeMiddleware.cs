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
            if (!_env.IsDevelopment())
            {
                await _next(context);
                return;
            }

            await _next(context);
            var collector = context.RequestServices.GetRequiredService<DbEyeCollector>();
            
            var path = context.Request.Path.ToString();

            var ms = options.Value.EndpointThresholds.TryGetValue(path, out var custom)
                ? custom : options.Value.SlowQueryThresholdMs;

            var thresHoldMs = TimeSpan.FromMilliseconds(ms);

            var sqlQueries = collector.Queries  
                                .GroupBy(s => s.Sql)
                                .Where(s => s.Count() > 1).ToList();

            var slowQueries = collector.Queries
                                .Where(d => d.DurationInMs >= thresHoldMs).ToList();
                            
            var separator = new string('-', 50);

            foreach (var item in sqlQueries)
            {
                _logger.LogWarning($"\n{separator}\n⚠️  N+1 detected at {context.Request.Method} {context.Request.Path}\nQuery repeated {item.Count()}x - {item.Key} \n\n{separator}");
            }

            foreach (var item in slowQueries)
            {
                _logger.LogWarning($"\n{separator}\n\n⚠️  Slow query detected at {context.Request.Method} {context.Request.Path}\nDuration: {item.DurationInMs.TotalMilliseconds}ms - {item.Sql} \n\n{separator}");
            }

            if (!sqlQueries.Any() && !slowQueries.Any())
                _logger.LogInformation($"\n{separator}\n\n✅  {context.Request.Method} {context.Request.Path} - no issues detected \n{separator}");
        }
    }
}