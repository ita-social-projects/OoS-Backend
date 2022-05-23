using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Extensions.Configuration;
using OutOfSchool.Common.Config;

namespace OutOfSchool.Common.Extensions.Startup
{
    public static class ConnectionStringExtensions
    {
        public static string GetMySqlConnectionString<TOptions>(
            this IConfiguration config,
            string connectionStringKey,
            Func<TOptions, DbConnectionStringBuilder> optionsToBuilder = null)
            where TOptions : IMySqlConnectionOptions
        {
            var overrides = config
                .GetSection("ConnectionStringsOverride")
                .Get<Dictionary<string, TOptions>>() ?? new Dictionary<string, TOptions>();

            DbConnectionStringBuilder connectionStringBuilder;

            TOptions options;
            if (overrides.TryGetValue(connectionStringKey, out options) && options.UseOverride)
            {
                if (optionsToBuilder == null)
                {
                    throw new ArgumentException("Please provide options to builder conversion if you are using overrides");
                }

                connectionStringBuilder = optionsToBuilder(options);
            }
            else
            {
                connectionStringBuilder = new DbConnectionStringBuilder()
                {
                    ConnectionString = config.GetConnectionString(connectionStringKey),
                };
            }

            if (string.IsNullOrEmpty(connectionStringBuilder.ConnectionString))
            {
                throw new Exception("Provide a valid connection string or options");
            }

            if (!connectionStringBuilder.ContainsKey("guidformat") || connectionStringBuilder["guidformat"].ToString().ToLower() != "binary16")
            {
                throw new Exception("The connection string should have a key: 'guidformat' and a value: 'binary16'");
            }

            return connectionStringBuilder.ConnectionString;
        }
    }
}