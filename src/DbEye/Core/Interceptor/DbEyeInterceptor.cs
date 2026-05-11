using System.Data.Common;
using DbEye.Common.Models;
using DbEye.Core.Collector;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DbEye.Core.Interceptor
{
    public class DbEyeInterceptor : DbCommandInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbEyeInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            Collect(command, eventData);
            return result;
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            Collect(command, eventData);
            return new ValueTask<DbDataReader>(result);
        }

        private void Collect(DbCommand command, CommandExecutedEventData eventData)
        {
            Console.WriteLine("Collect chamado!");
            if (eventData.Context is null)
                return;

            var newQuery = new QueryModel(
                command.CommandText,
                eventData.Duration
            );

            try
            {
                var collector = _httpContextAccessor.HttpContext?.RequestServices
                    .GetRequiredService<DbEyeCollector>()!;
                collector.Add(newQuery);
            }
            catch (Exception)
            {
                
            }
        }
    }
}