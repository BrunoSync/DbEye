using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Common.Options;
using DbEye.Core.Collector;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DbEye.Core.Middleware
{
    public class DbEyeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DbEyeMiddleware> _logger;

        public DbEyeMiddleware(RequestDelegate next, ILogger<DbEyeMiddleware> logger)
        {
            Console.WriteLine("DbEyeMiddleware instanciado!");
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<DbEyeOptions> options)
        {
            Console.WriteLine("Middleware chamado!");
            await _next(context);
            var collector = context.RequestServices.GetRequiredService<DbEyeCollector>();
            Console.WriteLine($"Queries no collector: {collector.Queries.Count}");
            var thresHoldMs = TimeSpan.FromMilliseconds(options.Value.SlowQueryThresholdMs);

            var sqlQueries = collector.Queries  
                                .GroupBy(s => s.Sql)
                                .Where(s => s.Count() > 1);

            var slowQueries = collector.Queries
                                .Where(d => d.DurationInMs >= thresHoldMs);

            foreach (var item in sqlQueries)
            {
                _logger.LogWarning($"⚠️ N+1 detectado em {context.Request.Method} {context.Request.Path}\nQuery repetida {item.Count()} - {item.Key}");
            }

            foreach (var item in slowQueries)
            {
                _logger.LogWarning($"⚠️ Query lenta detectada em {context.Request.Method} {context.Request.Path}\nDuração: {item.DurationInMs.TotalMilliseconds} - {item.Sql}");
            }
        }
    }
}