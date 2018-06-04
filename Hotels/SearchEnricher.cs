using Serilog.Core;
using Serilog.Events;

namespace Hotels
{
    public class SearchEnricher : ILogEventEnricher
    {
        private readonly SearchRequest _request;

        public SearchEnricher(SearchRequest request)
        {
            _request = request;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(new LogEventProperty("SearchId", new ScalarValue(_request.SearchId)));
            logEvent.AddPropertyIfAbsent(new LogEventProperty("VisitSourceId", new ScalarValue(_request.VisitSourceId)));
        }
    }
}