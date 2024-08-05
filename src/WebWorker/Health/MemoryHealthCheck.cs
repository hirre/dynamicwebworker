using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebWorker.Health
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        public MemoryHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Name => "memory_check";

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var threshold = long.TryParse(_configuration[Definitions.WEBWORKER_HEALTH_MEMORY_TRESHOLD], out var thresholdVal) ? thresholdVal : 1073741824L;

            // Include GC information in the reported diagnostics.
            var allocated = GC.GetTotalMemory(forceFullCollection: false);
            var data = new Dictionary<string, object>()
            {
                { "AllocatedBytes", allocated },
                { "Gen0Collections", GC.CollectionCount(0) },
                { "Gen1Collections", GC.CollectionCount(1) },
                { "Gen2Collections", GC.CollectionCount(2) },
            };

            var status = (allocated < threshold) ? HealthStatus.Healthy : HealthStatus.Unhealthy;

            return Task.FromResult(new HealthCheckResult(
                status,
                description: "Reports degraded status if allocated bytes " +
                    $">= {threshold} bytes.",
                exception: null,
                data: data));
        }
    }
}
