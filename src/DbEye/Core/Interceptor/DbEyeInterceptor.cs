using System.Data.Common;
using DbEye.Common.Models;
using DbEye.Common.Utils;
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

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            SlowQueryInjector.InjectSlowQuery(command, eventData);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            SlowQueryInjector.InjectSlowQuery(command, eventData);
            return ValueTask.FromResult(result);
        }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            Collect(command, eventData);
            return result;
        }

        public override ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            Collect(command, eventData);
            return new ValueTask<DbDataReader>(result);
        }

        private void Collect(DbCommand command, CommandExecutedEventData eventData)
        {
            if (eventData.Context is null)
                return;

            var sql = SlowQueryInjector.Strip(command.CommandText);

            var newQuery = new QueryModel(sql, eventData.Duration);

            try
            {
                var collector = _httpContextAccessor.HttpContext?.RequestServices
                    .GetRequiredService<DbEyeCollector>()!;
                collector.Add(newQuery);
            }
            catch (Exception) { }
        }
    }
}