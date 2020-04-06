using System.Collections.Generic;
using KubeClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sinker.HostedServices;
using Sinker.Common;

namespace Sinker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<SecretsWatcherHostedService>();
            services.AddControllers();
            var clientOptions = K8sConfig.Load(@"C:\Users\linjm\.kube\config").ToKubeClientOptions(
                defaultKubeNamespace: "default"
            );
            services.AddSingleton<KubeClientOptions>(clientOptions);

            var providers = new ISecretsProvider[]
            {
                new SecretsProvider()
            };
            services.AddSingleton<IEnumerable<ISecretsProvider>>(providers);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
