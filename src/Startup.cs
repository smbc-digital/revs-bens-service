using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using revs_bens_service.Utils.HealthChecks;
using revs_bens_service.Utils.ServiceCollectionExtensions;
using revs_bens_service.Utils.StorageProvider;

namespace revs_bens_service
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            
            services.AddGateways(Configuration)
                    .AddStorageProvider(Configuration)
                    .RegisterServices()
                    .AddSwagger()
                    .AddHealthChecks()
                    .AddCheck<TestHealthCheck>("TestHealthCheck");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) =>
            app.UseExceptionHandler($"/api/v1/error{(env.IsDevelopment() ? "/local" : string.Empty)}")
                .UseRouting()
                .UseEndpoints(endpoints => endpoints.MapControllers())
                .UseHealthChecks("/healthcheck", HealthCheckConfig.Options)
                .UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "Revs and Bens service API");
                });
    }
}