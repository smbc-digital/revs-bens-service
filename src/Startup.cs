using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using revs_bens_service.Services.Benefits;
using revs_bens_service.Services.CouncilTax;
using revs_bens_service.Services.Dashboard;
using revs_bens_service.Utils.HealthChecks;
using revs_bens_service.Utils.StorageProvider;
using StockportGovUK.AspNetCore.Availability;
using StockportGovUK.AspNetCore.Availability.Middleware;
using StockportGovUK.AspNetCore.Gateways;
using StockportGovUK.AspNetCore.Gateways.CivicaServiceGateway;
using StockportGovUK.AspNetCore.Middleware;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace revs_bens_service
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBenefitsService, BenefitsService>();
            services.AddSingleton<IPeopleService, PeopleService>();
            services.AddSingleton<ICouncilTaxService, CouncilTaxService>();
            services.AddSingleton<IBenefitsService, BenefitsService>();
            services.AddSingleton<ICivicaServiceGateway, CivicaServiceGateway>();
            services.AddStorageProvider(Configuration);

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddAvailability();
            services.AddResilientHttpClients<IGateway, Gateway>(Configuration);
            services.AddHealthChecks()
                .AddCheck<TestHealthCheck>("TestHealthCheck");
            services.AddHttpClient();
            services.AddStorageProvider(Configuration);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "revs_bens_service API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Authorization using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }},
                });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsEnvironment("local"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMiddleware<Availability>();
            app.UseMiddleware<ExceptionHandling>();
            app.UseHttpsRedirection();
            app.UseHealthChecks("/healthcheck", HealthCheckConfig.Options);
            app.UseSwagger();
            app.UseMvc();
            var swaggerPrefix = env.IsEnvironment("local") ? string.Empty : "/revsbensservice";
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"{swaggerPrefix}/swagger/v1/swagger.json", "revs_bens_service API");
            });
        }
    }
}
