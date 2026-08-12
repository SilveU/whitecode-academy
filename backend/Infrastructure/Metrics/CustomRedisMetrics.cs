using System.Diagnostics.Metrics;

namespace Infrastructure.Metrics
{
    public static class CustomRedisMetrics
    {
        private static readonly Meter _meter = new Meter("WhiteCodeAcademy.Redis");
        public static readonly Counter<long> Operation = _meter.CreateCounter<long>("redis.operations");

        public static readonly Counter<long> CacheHits = _meter.CreateCounter<long>("redis.cache.hits");

        public static readonly Counter<long> CacheMisses = _meter.CreateCounter<long>("redis.cache.misses");

        public static readonly Counter<long> Errors = _meter.CreateCounter<long>("redis.errors");

        public static readonly Histogram<double> OperationDuration = _meter.CreateHistogram<double>("redis.operation.duration", unit: "ms");
    }
}
