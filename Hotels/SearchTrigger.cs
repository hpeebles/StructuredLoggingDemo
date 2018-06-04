using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hotels
{
    public class SearchTrigger : BackgroundService
    {
        private readonly int _visitSource;
        private readonly SearchEngine _searchEngine;
        private readonly ILogger _logger;
        private static readonly Random Rng = new Random();

        public SearchTrigger(int visitSource, SearchEngine searchEngine, ILogger logger)
        {
            _visitSource = visitSource;
            _searchEngine = searchEngine;
            _logger = logger;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_visitSource * -Math.Log(Rng.NextDouble())));

                    var estabIds = Enumerable
                        .Range(1, Rng.Next(100))
                        .Where(id => Rng.NextDouble() > 1 - Math.Pow(0.99, id))
                        .ToList();

                    var request = new SearchRequest
                    {
                        SearchId = Guid.NewGuid(),
                        VisitSourceId = _visitSource,
                        EstabIds = new HashSet<int>(estabIds)
                    };

                    await _searchEngine.Search(request);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Search error");
                }
            }
        }
    }
}