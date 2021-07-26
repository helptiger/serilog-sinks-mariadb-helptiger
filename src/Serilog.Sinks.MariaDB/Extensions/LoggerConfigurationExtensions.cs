﻿using Nerdle.AutoConfig;

using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.MariaDB.Sinks;

using System;

namespace Serilog.Sinks.MariaDB.Extensions
{
    public static class LoggerConfigurationExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events to MariaDB/MySQL table
        /// </summary>
        /// <param name="loggerConfiguration">Options for the sink</param>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="formatProvider">The format provider used to supply culture-specific formatting information</param>
        /// <param name="batchPostingLimit">The maximum number of events to include in a single batch</param>
        /// <param name="queueSizeLimit">The maximum number of events that will be held in-memory while waiting to store them to SQL. Beyond this limit, events will be dropped. Default is 10000</param>   
        /// <param name="period">The time to wait between checking for event batches</param>
        /// <param name="options">Additional options for the sink</param>
        /// <param name="tableName">Name of the database table used for storing events</param>
        /// <param name="autoCreateTable">If true the sink will create a SQL table if it doesn't exist</param>
        /// <param name="useBulkInsert">If true, tries to insert whole buffer of event collected per <paramref name="period"/> with a single command (more efficient, but less reliable)</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink</param>
        public static LoggerConfiguration MariaDB(
            this LoggerSinkConfiguration loggerConfiguration,
            string connectionString,
            IFormatProvider formatProvider = null,
            int batchPostingLimit = MariaDBSink.DefaultBatchPostingLimit,
            int queueSizeLimit = 10000,
            TimeSpan? period = null,
            MariaDBSinkOptions options = null,
            string tableName = "Logs",
            bool autoCreateTable = false,
            bool useBulkInsert = true,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum
            )
        {
            if (loggerConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerConfiguration));
            }

            return loggerConfiguration.Sink(
                new MariaDBSink(
                    connectionString,
                    formatProvider,
                    batchPostingLimit,
                    queueSizeLimit,
                    period ?? MariaDBSink.DefaultPeriod,
                    GetMariaDBSinkOptions(options),
                    tableName,
                    autoCreateTable,
                    useBulkInsert
                ),
                restrictedToMinimumLevel
            );
        }

        /// <summary>
        /// Adds a sink that writes audit events to MariaDB/MySQL table
        /// </summary>
        /// <param name="loggerAuditConfiguration">Options for the sink</param>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="formatProvider">The format provider used to supply culture-specific formatting information</param>
        /// <param name="options">Additional options for audit sink</param>
        /// <param name="tableName">Name of the database table used for storing events</param>
        /// <param name="autoCreateTable">If true the sink will create a SQL table if it doesn't exist</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink</param>
        public static LoggerConfiguration MariaDB(
            this LoggerAuditSinkConfiguration loggerAuditConfiguration,
            string connectionString,
            IFormatProvider formatProvider = null,
            MariaDBSinkOptions options = null,
            string tableName = "Logs",
            bool autoCreateTable = false,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum
            )
        {
            if (loggerAuditConfiguration == null)
            {
                throw new ArgumentNullException(nameof(loggerAuditConfiguration));
            }

            return loggerAuditConfiguration.Sink(
                new MariaDBAuditSink(
                    connectionString,
                    formatProvider,
                    GetMariaDBSinkOptions(options),
                    tableName,
                    autoCreateTable
                ),
                restrictedToMinimumLevel
            );
        }

        private static MariaDBSinkOptions GetMariaDBSinkOptions(MariaDBSinkOptions options)
        {
#if NETFRAMEWORK
            if (options != null)
                return options;

            try
            {
                var dynamicSinkOptions = AutoConfig.Map<IMariaDBSinkOptions>();
                return new MariaDBSinkOptions()
                {
                    DeleteChunkSize = dynamicSinkOptions.DeleteChunkSize,
                    EnumsAsInts = dynamicSinkOptions.EnumsAsInts,
                    ExcludePropertiesWithDedicatedColumn = dynamicSinkOptions.ExcludePropertiesWithDedicatedColumn,
                    HashMessageTemplate = dynamicSinkOptions.HashMessageTemplate,
                    LogRecordsCleanupFrequency = dynamicSinkOptions.LogRecordsCleanupFrequency,
                    LogRecordsExpiration = dynamicSinkOptions.LogRecordsExpiration,
                    TimestampInUtc = dynamicSinkOptions.TimestampInUtc,
                    PropertiesToColumnsMapping = dynamicSinkOptions.PropertiesToColumnsMapping,
                };
            }
            catch (Exception e)
            {
                // Ignore
            }

            return new MariaDBSinkOptions();
#else
            return options ?? new MariaDBSinkOptions();
#endif
        }
    }
}
