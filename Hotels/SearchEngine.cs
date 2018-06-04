using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;

namespace Hotels
{
    public class SearchEngine
    {
        private readonly IList<Supplier> _suppliers;
        private readonly ILogger _logger;

        public SearchEngine(IList<Supplier> suppliers, ILogger logger)
        {
            _suppliers = suppliers;
            _logger = logger;
        }

        public async Task<ISet<int>> Search(SearchRequest request)
        {
            var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var timer = Stopwatch.StartNew();

            using (LogContext.Push(new SearchEnricher(request)))
            {
                _logger.Information("Starting search {SearchId}", request.SearchId);
                
                var tasks = _suppliers
                    .Select(s => s.GetAvailability(request.EstabIds, timeout.Token))
                    .ToList();

                await Task.WhenAll(tasks);

                var results = new HashSet<int>(tasks
                    .Where(t => t?.Result?.EstabIds != null)
                    .SelectMany(t => t.Result.EstabIds));

                _logger.Information(
                    "Search completed in {Elapsed:0.00}ms. {ResultCount} result(s) found",
                    timer.Elapsed.TotalMilliseconds,
                    results.Count);

                return results;
            }
        }
    }
}