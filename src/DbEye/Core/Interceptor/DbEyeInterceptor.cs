using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using DbEye.Common.Models;
using DbEye.Core.Collector;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DbEye.Core.Interceptor
{
    public class DbEyeInterceptor : DbCommandInterceptor
    {
        private readonly IServiceScopeFactory _factory;

        public DbEyeInterceptor(IServiceScopeFactory factory)
        {
            _factory = factory;
        }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            if (eventData.Context is null)
                return result;

            var newQuery = new QueryModel(
                command.CommandText,
                eventData.Duration
            );

            var serviceProvider = ((IInfrastructure<IServiceProvider>)eventData.Context).Instance;
            var collector = serviceProvider.GetRequiredService<DbEyeCollector>();

            collector.Add(newQuery);

            return result;
        }
    }
}