using Serilog.Core;
using Serilog.Events;

namespace GgStatAggregator.Logger
{
    public class UtcTimestampEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(
                propertyFactory.CreateProperty("UtcTimestamp", DateTime.UtcNow));
        }
    }
}
