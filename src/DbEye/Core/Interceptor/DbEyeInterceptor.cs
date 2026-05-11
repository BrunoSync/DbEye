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

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            InjectSlowQuery(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            InjectSlowQuery(command);
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

        private static void InjectSlowQuery(DbCommand command)
        {
            if (command.CommandText.Contains("-- db-eye-slow-simulate"))
            {
                command.CommandText = $"DO $$ BEGIN PERFORM pg_sleep(1); END $$;\n{command.CommandText}";
            }
        }

        private void Collect(DbCommand command, CommandExecutedEventData eventData)
        {
            if (eventData.Context is null)
                return;

            var sql = command.CommandText;

            if (sql.Contains("-- db-eye-slow-simulate"))
            {
                sql = sql
                    .Replace("DO $$ BEGIN PERFORM pg_sleep(1); END $$;\n", "")
                    .Replace("-- db-eye-slow-simulate\n\n", "")
                    .Trim();
            }

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