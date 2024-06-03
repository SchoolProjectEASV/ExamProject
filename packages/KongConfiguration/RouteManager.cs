using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KongSetup
{
    /// <summary>
    /// Class that creates or updates routes in Kong.
    /// </summary>
    public class RouteManager
    {
        private readonly KongClient _kongClient;

        public RouteManager(KongClient kongClient)
        {
            _kongClient = kongClient;
        }

        /// <summary>
        /// Adds or updates a route. If it already exists, it updates the route.
        /// </summary>
        /// <param name="serviceName">The service, that the route is being added to</param>
        /// <param name="path">Path for the route</param>
        /// <param name="methods">The methods the routes takes</param>
        /// <param name="bypassAuth">Whether or not auth shall be placed on the specific route</param>
        /// <returns></returns>

        public async Task AddOrUpdateRoute(string serviceName, string path, string[] methods, bool bypassAuth)
        {
            var routeData = new
            {
                paths = new[] { path },
                methods = methods,
                preserve_host = true,
                strip_path = false
            };

            var routeResponse = await _kongClient.GetAsync($"services/{serviceName}/routes");
            var routesContent = await routeResponse.Content.ReadAsStringAsync();
            dynamic routes = JsonConvert.DeserializeObject(routesContent);

            bool routeExists = false;
            string routeId = null;

            foreach (var route in routes.data)
            {
                if (route.paths[0] == path)
                {
                    routeExists = true;
                    routeId = route.id;
                    break;
                }
            }

            if (routeExists)
            {
                var updateResponse = await _kongClient.PatchAsync($"routes/{routeId}", routeData);
                updateResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Route for service {serviceName} at {path} updated.");
            }
            else
            {
                var createResponse = await _kongClient.PostAsync($"services/{serviceName}/routes", routeData);
                createResponse.EnsureSuccessStatusCode();
                var createResponseContent = await createResponse.Content.ReadAsStringAsync();
                dynamic createResponseJson = JsonConvert.DeserializeObject(createResponseContent);
                routeId = createResponseJson.id;
                Console.WriteLine($"Route for service {serviceName} added at {path}.");
            }

            if (!path.StartsWith("/auth") && !bypassAuth)
            {
                await EnsureJwtOnRoute(routeId);
            }
        }

        /// <summary>
        /// Adds JWT authentication to a route if it is not already enabled.
        /// </summary>
        /// <param name="routeId">RouteID for the route</param>
        /// <returns></returns>
        private async Task EnsureJwtOnRoute(string routeId)
        {
            var pluginsResponse = await _kongClient.GetAsync($"routes/{routeId}/plugins");
            var pluginsContent = await pluginsResponse.Content.ReadAsStringAsync();
            dynamic plugins = JsonConvert.DeserializeObject(pluginsContent);

            foreach (var plugin in plugins.data)
            {
                if (plugin.name == "jwt")
                {
                    Console.WriteLine($"JWT authentication is already enabled on route {routeId}.");
                    return;
                }
            }

            var jwtAuthData = new
            {
                name = "jwt",
                config = new
                {
                    key_claim_name = "iss",
                    secret_is_base64 = false
                }
            };
            var jwtAuthResponse = await _kongClient.PostAsync($"routes/{routeId}/plugins", jwtAuthData);
            jwtAuthResponse.EnsureSuccessStatusCode();
        }
    }
}
