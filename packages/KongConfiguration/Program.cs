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

        static void Main(string[] args)
        {
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");

            var kongAdminUrl = "http://kong:8001";

            var configPath = "packages/KongConfiguration/kong-config.json";
            var config = LoadConfig(configPath);

            foreach (var service in config.Services)
            {
                AddOrUpdateService(kongAdminUrl, service.Name, service.Url).Wait();

                foreach (var route in service.Routes)
                {
                    AddOrUpdateRoute(kongAdminUrl, service.Name, route.Path, route.Methods).Wait();
                }
            }

            Console.WriteLine("Services and routes have been ensured in Kong.");
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

        private static async Task AddOrUpdateRoute(string kongAdminUrl, string serviceName, string path, string[] methods)
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
                Console.WriteLine($"Route for service {serviceName} added at {path}.");
            }
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
    }
}
