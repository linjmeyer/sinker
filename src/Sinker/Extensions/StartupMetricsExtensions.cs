using System.Linq;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Microsoft.AspNetCore.Hosting;

namespace Sinker.Extensions
{
    internal static class StartupMetricsExtensions
    {
        public static IMetricsRoot Metrics { get; set; } = AppMetrics.CreateDefaultBuilder()
            .OutputMetrics.AsPrometheusPlainText()
            .OutputMetrics.AsPrometheusProtobuf()
            .Build();

        public static IWebHostBuilder UseSinkerAppMetrics(this IWebHostBuilder builder)
        {
            return builder
                .UseMetricsWebTracking()
                .ConfigureAppMetricsHostingConfiguration(options =>
                {
                })
                .UseMetrics(options =>
                {
                    options.EndpointOptions = endpointsOptions =>
                    {
                        // /metrics = prometheus protobuf
                        endpointsOptions.MetricsEndpointOutputFormatter = 
                            Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusProtobufOutputFormatter>().First();
                            
                        // /metrics-text = prometheus text
                        endpointsOptions.MetricsTextEndpointOutputFormatter = 
                            Metrics.OutputMetricsFormatters.OfType<MetricsPrometheusTextOutputFormatter>().First();
                    };
                });
        }
    }
}