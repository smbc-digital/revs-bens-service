using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace revs_bens_service.Utils.StorageProvider
{
    public static class StorageProviderExtension
    {
        public static void AddStorageProvider(this IServiceCollection services, IConfiguration configuration)
        {
            var storageProviderConfiguration = configuration.GetSection("StorageProvider");

            switch (storageProviderConfiguration["Type"])
            {
                case "Redis":
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.InstanceName = storageProviderConfiguration["Name"] ?? "master";
                        options.Configuration = storageProviderConfiguration["Address"] ?? "http://127.0.0.1";
                    });
                    break;
                default:
                    services.AddDistributedMemoryCache();
                    break;
            }

            services.AddSingleton<ICacheProvider, CacheProvider>();
        }
    }
}
