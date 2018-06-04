using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Hotels
{
    public class Supplier
    {
        private readonly int _supplierId;
        private readonly TimeSpan _averageSearchDuration;
        private readonly double _availabilityMultiplier;
        private readonly double _errorPercentage;
        private readonly ILogger _logger;
        private static readonly Random Rng = new Random();
        private static bool _hasWeirdEventHappened;

        public Supplier(int supplierId, TimeSpan averageSearchDuration, double availabilityMultiplier, double errorPercentage, ILogger logger)
        {
            _supplierId = supplierId;
            _averageSearchDuration = averageSearchDuration;
            _availabilityMultiplier = availabilityMultiplier;
            _errorPercentage = errorPercentage;
            _logger = logger.ForContext("SupplierId", _supplierId);
        }
        
        public async Task<SupplierSearchResults> GetAvailability(ISet<int> estabIds, CancellationToken token)
        {
            var timer = Stopwatch.StartNew();
         
            var results = new SupplierSearchResults { SupplierId = _supplierId };
            
            _logger
                .ForContext("EstabId", estabIds)
                .Information("Supplier search starting. SupplierId {SupplierId}", _supplierId);

            try
            {
                await Task.Delay(GetSearchDuration(), token);
            }
            catch (OperationCanceledException)
            {
                _logger.Warning("Supplier search timeout. SupplierId {SupplierId}", _supplierId);
                return results;
            }

            if (IsError())
            {
                _logger.Error("Supplier search error. SupplierId {SupplierId}", _supplierId);
                return results;
            }

            results.EstabIds = new HashSet<int>(estabIds.Where(IsAvailable));

            if (!_hasWeirdEventHappened && Rng.NextDouble() < 0.00001)
            {
                _logger.Information("Something weird happened!");
                _hasWeirdEventHappened = true;
            }
            
            _logger
                .ForContext("EstabId", results.EstabIds)
                .Information(
                    "Supplier search took {Elapsed:0.00}ms and returned {ResultCount} result(s). SupplierId {SupplierId}",
                    timer.Elapsed.TotalMilliseconds,
                    results.EstabIds.Count,
                    _supplierId);

            return results;
        }

        private bool IsAvailable(int estabId)
        {
            return Math.Pow(estabId, -0.5) * _availabilityMultiplier > Rng.NextDouble();
        }

        private bool IsError()
        {
            return Rng.NextDouble() < _errorPercentage / 100;
        }

        private TimeSpan GetSearchDuration()
        {
            return TimeSpan.FromMilliseconds(_averageSearchDuration.TotalMilliseconds * -Math.Log(Rng.NextDouble()));
        }
    }

    public class SupplierSearchResults
    {
        public int SupplierId;
        public ISet<int> EstabIds;
    }
}