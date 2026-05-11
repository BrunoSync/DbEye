using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Common.Options;
using DbEye.Core.Collector;
using DbEye.Core.Interceptor;
using DbEye.Core.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DbEye.Common.Extensions
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDbEye(this IServiceCollection services, Action<DbEyeOptions> configure = null!)
        {
            services.AddScoped<DbEyeCollector>();
            services.AddSingleton<DbEyeInterceptor>();
            services.Configure<DbEyeOptions>(configure ?? (_ => {}));
            services.AddHttpContextAccessor();

            return services;
        }

        public static IApplicationBuilder UseDbEye(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<DbEyeMiddleware>();

            return builder;
        }
    }
}