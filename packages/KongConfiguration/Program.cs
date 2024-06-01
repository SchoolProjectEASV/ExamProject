using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KongSetup
{
    class Program
    {
        private static readonly HttpClient client;

        static Program()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            client = new HttpClient(handler);
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

            var kongAdminUrl = "http://kong:8001";

            var configPath = "packages/KongConfiguration/kong-config.json";
            var config = LoadConfig(configPath);

            foreach (var service in config.Services)
            {
                await AddOrUpdateService(kongAdminUrl, service.Name, service.Url);

                foreach (var route in service.Routes)
                {
                    await AddOrUpdateRoute(kongAdminUrl, service.Name, route.Path, route.Methods, route.BypassAuth);
                }
            }

            // Enable global rate limiting
            await EnableGlobalRateLimiting(kongAdminUrl);

            // Set up JWT Authentication
            await SetupJwtAuth(kongAdminUrl);

            Console.WriteLine("Services, routes, global rate limiting, and JWT auth have been ensured in Kong.");
        }

        private static Config LoadConfig(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Config>(json);
        }

        private static async Task AddOrUpdateService(string kongAdminUrl, string name, string url)
        {
            var serviceData = new
            {
                name = name,
                url = url,
                connect_timeout = 60000,
                read_timeout = 60000,
                write_timeout = 60000,
                retries = 5
            };

            var content = new StringContent(JsonConvert.SerializeObject(serviceData), Encoding.UTF8, "application/json");

            var serviceResponse = await client.GetAsync($"{kongAdminUrl}/services/{name}");
            if (serviceResponse.IsSuccessStatusCode)
            {
                var updateContent = new StringContent(JsonConvert.SerializeObject(new
                {
                    url = url
                }), Encoding.UTF8, "application/json");

                var updateResponse = await client.PatchAsync($"{kongAdminUrl}/services/{name}", updateContent);
                updateResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Service {name} updated.");
            }
            else
            {
                var createResponse = await client.PostAsync($"{kongAdminUrl}/services", content);
                createResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Service {name} added.");
            }
        }

        private static async Task AddOrUpdateRoute(string kongAdminUrl, string serviceName, string path, string[] methods, bool bypassAuth)
        {
            var routeData = new
            {
                paths = new[] { path },
                methods = methods,
                preserve_host = true,
                strip_path = false
            };

            var content = new StringContent(JsonConvert.SerializeObject(routeData), Encoding.UTF8, "application/json");

            var routeResponse = await client.GetAsync($"{kongAdminUrl}/services/{serviceName}/routes");
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
                var updateResponse = await client.PatchAsync($"{kongAdminUrl}/routes/{routeId}", content);
                updateResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Route for service {serviceName} at {path} updated.");
            }
            else
            {
                var createResponse = await client.PostAsync($"{kongAdminUrl}/services/{serviceName}/routes", content);
                createResponse.EnsureSuccessStatusCode();
                var createResponseContent = await createResponse.Content.ReadAsStringAsync();
                dynamic createResponseJson = JsonConvert.DeserializeObject(createResponseContent);
                routeId = createResponseJson.id;
                Console.WriteLine($"Route for service {serviceName} added at {path}.");
            }

            if (!bypassAuth)
            {
                await EnsureJwtOnRoute(kongAdminUrl, routeId);
            }
        }

        private static async Task EnableGlobalRateLimiting(string kongAdminUrl)
        {
            var rateLimitingData = new
            {
                name = "rate-limiting",
                config = new
                {
                    minute = 5,
                    policy = "local"
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(rateLimitingData), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{kongAdminUrl}/plugins", content);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Global rate limiting enabled.");
        }

        private static async Task SetupJwtAuth(string kongAdminUrl)
        {
            // Create a new consumer
            var consumerData = new
            {
                username = "luka"
            };

            var consumerContent = new StringContent(JsonConvert.SerializeObject(consumerData), Encoding.UTF8, "application/json");
            var consumerResponse = await client.PostAsync($"{kongAdminUrl}/consumers", consumerContent);
            consumerResponse.EnsureSuccessStatusCode();
            Console.WriteLine("Consumer 'luka' created.");

            // Create a JWT credential for the consumer
            var jwtCredentialData = new
            {
                key = "luka-key",
                secret = "PELLEDRAGSTEDSKALVÆREDANMARKSTATSMINISTER2024"
            };

            var jwtCredentialContent = new StringContent(JsonConvert.SerializeObject(jwtCredentialData), Encoding.UTF8, "application/json");
            var jwtCredentialResponse = await client.PostAsync($"{kongAdminUrl}/consumers/luka/jwt", jwtCredentialContent);
            jwtCredentialResponse.EnsureSuccessStatusCode();
            Console.WriteLine("JWT credentials created for consumer 'luka'.");
        }

        private static async Task EnsureJwtOnRoute(string kongAdminUrl, string routeId)
        {
            var pluginsResponse = await client.GetAsync($"{kongAdminUrl}/routes/{routeId}/plugins");
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

            var jwtAuthContent = new StringContent(JsonConvert.SerializeObject(jwtAuthData), Encoding.UTF8, "application/json");
            var jwtAuthResponse = await client.PostAsync($"{kongAdminUrl}/routes/{routeId}/plugins", jwtAuthContent);
            jwtAuthResponse.EnsureSuccessStatusCode();
            Console.WriteLine($"JWT authentication enabled on route {routeId}.");
        }
    }

    public class Config
    {
        public List<Service> Services { get; set; }
    }

    public class Service
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public List<Route> Routes { get; set; }
    }

    public class Route
    {
        public string Path { get; set; }
        public string[] Methods { get; set; }
        public bool BypassAuth { get; set; }
    }
}
