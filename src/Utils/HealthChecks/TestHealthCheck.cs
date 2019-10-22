using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace revs_bens_service.Utils.HealthChecks
{
    public class TestHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return HealthCheckResult.Healthy(null, new Dictionary<string, object> {{"Result", "All working!"}});
        }
    }
}