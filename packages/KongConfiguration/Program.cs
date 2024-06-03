using System;
using System.Threading.Tasks;
using KongSetup.KongEntities;
using Newtonsoft.Json;
using System.IO;

namespace KongSetup
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var kongAdminUrl = "http://kong:8001";
            var configPath = "packages/KongConfiguration/kong-config.json";

            var config = ConfigLoader.LoadConfig(configPath);

            var kongClient = new KongClient(kongAdminUrl);
            var serviceManager = new ServiceManager(kongClient);
            var routeManager = new RouteManager(kongClient);
            var authSetup = new AuthSetup(kongClient);

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

            await authSetup.EnableGlobalRateLimiting();
            await authSetup.SetupJwtAuth();

            Console.WriteLine("Services, routes, global rate limiting, and JWT auth have been ensured in Kong.");
        }
    }
}
