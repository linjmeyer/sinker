using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Sinker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseSinkerAppMetrics();
                     // AllowSynchronousIO necessary for App.Metrics
                    webBuilder.UseKestrel(kestrel => kestrel.AllowSynchronousIO = true);
                });
    }
}