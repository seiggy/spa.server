using Microsoft.Extensions.Configuration;
using spa.server.Model;

namespace spa.server.Extensions
{
    public static class HealthChecks
    {
        public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            List<ApiMonitorOptions> apiHealthChecks = configuration.GetSection("App:Settings:ApiHealthChecks").Get<List<ApiMonitorOptions>>();
            
            services.AddHealthChecks();
            foreach (var apiHealthCheck in apiHealthChecks)
            {
                services.AddHealthChecks()
                    .AddUrlGroup(new Uri(apiHealthCheck.Url), name: apiHealthCheck.Name);
            }
            services.AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(10);
                setup.MaximumHistoryEntriesPerEndpoint(60);
                setup.SetApiMaxActiveRequests(1);
                setup.AddHealthCheckEndpoint("SPA Host", configuration["App:Settings:HealthChecksEndpoint"]!);
            }).AddInMemoryStorage();
        }
    }
}
