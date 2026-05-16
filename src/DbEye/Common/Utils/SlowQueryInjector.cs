using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DbEye.Common.Utils
{
    public static class SlowQueryInjector
    {
        public static void InjectSlowQuery(DbCommand command, CommandEventData eventData)
        {
            if (eventData.Context is null)
                return;
            
            var provider = eventData.Context.Database.ProviderName;

            var sql = command.CommandText;

            if (sql.Contains("-- db-eye-slow-simulate"))
            {
                var delayCommand = GetSlowQueryCommand(provider);
                if (delayCommand is not null)
                    command.CommandText = $"{delayCommand}\n{command.CommandText}";
            }
        }

        private static string? GetSlowQueryCommand(string? providerName) => providerName switch
        {
            "Microsoft.EntityFrameworkCore.SqlServer" => "WAITFOR DELAY '00:00:01';",
            "Npgsql.EntityFrameworkCore.PostgreSQL" => "DO $$ BEGIN PERFORM pg_sleep(1); END $$;",
            "Microsoft.EntityFrameworkCore.Sqlite" => "SELECT randomblob(10000000);",
            "Pomelo.EntityFrameworkCore.MySql" => "SELECT SLEEP(1);",
            "Oracle.EntityFrameworkCore" => "BEGIN DBMS_LOCK.SLEEP(1); END;",
            _ => null
        };

        public static string Strip(string sql)
        {
            if (!sql.Contains("-- db-eye-slow-simulate"))
                return sql;

            return sql
                .Replace("WAITFOR DELAY '00:00:01';\n", "")
                .Replace("DO $$ BEGIN PERFORM pg_sleep(1); END $$;\n", "")
                .Replace("SELECT randomblob(10000000);\n", "")
                .Replace("SELECT SLEEP(1);\n", "")
                .Replace("BEGIN DBMS_LOCK.SLEEP(1); END;\n", "")
                .Replace("-- db-eye-slow-simulate\n\n", "")
                .Replace("-- db-eye-slow-simulate\n", "")
                .Trim();
        }
    }
}