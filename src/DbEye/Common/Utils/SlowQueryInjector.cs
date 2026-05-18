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

            var sql = command.CommandText;

            if (sql.Contains("-- db-eye-slow-simulate"))
            {
                var delayCommand = "DO $$ BEGIN PERFORM pg_sleep(1); END $$;";
                command.CommandText = $"{delayCommand}\n{command.CommandText}";
            }
        }

        public static string Strip(string sql)
        {
            if (!sql.Contains("-- db-eye-slow-simulate"))
                return sql;

            return sql
                .Replace("DO $$ BEGIN PERFORM pg_sleep(1); END $$;", "");
        }
    }
}