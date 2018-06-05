using System;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Samples
{
    public class LoggingSamples
    {
        private readonly ILogger _logger;
        
        public LoggingSamples()
        {
            _logger = Log.Logger;
        }

        public void MessageTemplateDemo()
        {
            var random = new Random();
            
            _logger.Information("The time is {Time:yyyy-MM-dd hh:mm:ss} and my favourite number is {Number}", DateTime.UtcNow, random.Next(1, 100));
        }

        public void ForContextDemo()
        {
            // one off message with context
            _logger
                .ForContext("Day", DateTime.UtcNow.DayOfWeek)
                .Information("The time is {Time:yyyy-MM-dd hh:mm:ss}", DateTime.UtcNow);

            var correlationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            _logger
                .ForContext(new DemoEnricher(correlationId, userId))
                .Information("The time is {Time:yyyy-MM-dd hh:mm:ss}", DateTime.UtcNow);
            
            // new logger with context
            var logger = _logger.ForContext("ABC", 123);
            logger.Information("foo");
            logger.Information("bar");
        }

        public void LogContextDemo()
        {
            var correlationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                // all log messages within this scope will contain the correlationId
            }
            
            using (LogContext.Push(new DemoEnricher(correlationId, userId)))
            {
                // all log messages within this scope will apply the above context
            }
        }
    }
    
    public class DemoEnricher : ILogEventEnricher
    {
        private readonly Guid _correlationId;
        private readonly Guid _userId;

        public DemoEnricher(Guid correlationId, Guid userId)
        {
            _correlationId = correlationId;
            _userId = userId;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(new LogEventProperty("CorrelationId", new ScalarValue(_correlationId)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty("UserId", new ScalarValue(_userId)));
            
            if (logEvent.Level == LogEventLevel.Error)
                logEvent.AddPropertyIfAbsent(new LogEventProperty("ABC", new ScalarValue(123)));
        }
    }
}