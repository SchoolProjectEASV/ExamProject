using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using KongSetup.KongEntities;

namespace KongSetup
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton(sp => configuration.GetSection("KongSettings").Get<Settings>())
                .AddSingleton(sp => new KongClient("http://kong:8001"))
                .AddSingleton<UpstreamManager>() 
                .AddSingleton<RouteManager>()
                .AddSingleton<PluginManager>()
                .BuildServiceProvider();

            var kongClient = serviceProvider.GetService<KongClient>();
            var configPath = "kong-config.json";
            var config = ConfigLoader.LoadConfig(configPath);

            var serviceManager = serviceProvider.GetService<UpstreamManager>();
            var routeManager = serviceProvider.GetService<RouteManager>();
            var pluginManager = serviceProvider.GetService<PluginManager>();

            foreach (var service in config.Services)
            {
                if (service.Upstream != null)
                {
                    await serviceManager.AddOrUpdateUpstream(service.Upstream.Name);

                    foreach (var target in service.Upstream.Targets)
                    {
                        await serviceManager.AddTargetToUpstream(service.Upstream.Name, target);
                    }

                    await serviceManager.AddOrUpdateService(service.Name, service.Upstream.Name);
                }
                else
                {
                    await serviceManager.AddOrUpdateService(service.Name, service.Url);
                }

                foreach (var route in service.Routes)
                {
                    await routeManager.AddOrUpdateRoute(service.Name, route.Path, route.Methods, route.BypassAuth);
                }
            }

            await pluginManager.EnableGlobalRateLimiting();
            await pluginManager.SetupJwtAuth();

            Console.WriteLine("Services, routes, global rate limiting, and JWT auth have been ensured in Kong.");
        }
    }
}
