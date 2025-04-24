using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;

namespace GgStatAggregator.Logger
{
    public static class LoggerUtility
    {
        public static Serilog.ILogger ConfigureLogger(string defaultConnection) => new LoggerConfiguration()
            .Enrich.With(new UtcTimestampEnricher())
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Information()
            .WriteTo.MSSqlServer(
                connectionString: defaultConnection,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    AutoCreateSqlTable = true
                },
                columnOptions: new ColumnOptions
                {
                    AdditionalColumns =
                    [
                        new SqlColumn
                        {
                            ColumnName = "UtcTimestamp",
                            DataType = System.Data.SqlDbType.DateTime2,
                        }
                    ],
                    Store =
                    [
                        StandardColumn.Message,
                        StandardColumn.Level,
                        StandardColumn.Exception,
                        StandardColumn.Properties
                    ]
                }
            )
            .CreateLogger();
    }
}
