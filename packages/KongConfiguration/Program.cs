using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KongSetup.KongEntities;
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
            var kongAdminUrl = "http://kong:8001";
            var configPath = "packages/KongConfiguration/kong-config.json";
            var config = LoadConfig(configPath);

            foreach (var service in config.Services)
            {
                if (service.Upstream != null)
                {
                    await AddOrUpdateUpstream(kongAdminUrl, service.Upstream.Name);

                    foreach (var target in service.Upstream.Targets)
                    {
                        await AddTargetToUpstream(kongAdminUrl, service.Upstream.Name, target);
                    }

                    await AddOrUpdateService(kongAdminUrl, service.Name, service.Upstream.Name);
                }
                else
                {
                    await AddOrUpdateService(kongAdminUrl, service.Name, service.Url);
                }

                foreach (var route in service.Routes)
                {
                    await AddOrUpdateRoute(kongAdminUrl, service.Name, route.Path, route.Methods, route.BypassAuth);
                }
            }

            await EnableGlobalRateLimiting(kongAdminUrl);
            await SetupJwtAuth(kongAdminUrl);

            Console.WriteLine("Services, routes, global rate limiting, and JWT auth have been ensured in Kong.");
        }

        private static Config LoadConfig(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Config>(json);
        }

        private static async Task AddOrUpdateUpstream(string kongAdminUrl, string upstreamName)
        {
            var upstreamData = new
            {
                name = upstreamName
            };

            var content = new StringContent(JsonConvert.SerializeObject(upstreamData), Encoding.UTF8, "application/json");

            var upstreamResponse = await client.GetAsync($"{kongAdminUrl}/upstreams/{upstreamName}");
            if (upstreamResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Upstream {upstreamName} already exists.");
            }
            else
            {
                var createResponse = await client.PostAsync($"{kongAdminUrl}/upstreams", content);
                createResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Upstream {upstreamName} created.");
            }
        }

        private static async Task AddTargetToUpstream(string kongAdminUrl, string upstreamName, string target)
        {
            var targetResponse = await client.GetAsync($"{kongAdminUrl}/upstreams/{upstreamName}/targets");
            targetResponse.EnsureSuccessStatusCode();
            var targetContent = await targetResponse.Content.ReadAsStringAsync();
            dynamic existingTargets = JsonConvert.DeserializeObject(targetContent);

            foreach (var existingTarget in existingTargets.data)
            {
                if (existingTarget.target == target)
                {
                    Console.WriteLine($"Target {target} already exists in upstream {upstreamName}.");
                    return;
                }
            }

            var targetData = new
            {
                target = target
            };

            var content = new StringContent(JsonConvert.SerializeObject(targetData), Encoding.UTF8, "application/json");

            var createResponse = await client.PostAsync($"{kongAdminUrl}/upstreams/{upstreamName}/targets", content);
            createResponse.EnsureSuccessStatusCode();
            Console.WriteLine($"Target {target} added to upstream {upstreamName}.");
        }


        private static async Task AddOrUpdateService(string kongAdminUrl, string name, string upstreamName)
        {
            var serviceData = new
            {
                name = name,
                host = upstreamName,
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
                    host = upstreamName
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

            if (!path.StartsWith("/auth") && !bypassAuth)
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

            var getResponse = await client.GetAsync($"{kongAdminUrl}/plugins");
            var pluginsContent = await getResponse.Content.ReadAsStringAsync();
            dynamic plugins = JsonConvert.DeserializeObject(pluginsContent);

            foreach (var plugin in plugins.data)
            {
                if (plugin.name == "rate-limiting")
                {
                    Console.WriteLine("Global rate limiting is already enabled.");
                    return;
                }
            }
            var content = new StringContent(JsonConvert.SerializeObject(rateLimitingData), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{kongAdminUrl}/plugins", content);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Global rate limiting enabled.");
        }

        private static async Task SetupJwtAuth(string kongAdminUrl)
        {
            var consumerUsername = "jens";

            var consumerResponse = await client.GetAsync($"{kongAdminUrl}/consumers/{consumerUsername}");
            if (consumerResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var consumerData = new
                {
                    username = consumerUsername
                };

                var consumerContent = new StringContent(JsonConvert.SerializeObject(consumerData), Encoding.UTF8, "application/json");
                var createConsumerResponse = await client.PostAsync($"{kongAdminUrl}/consumers", consumerContent);
                createConsumerResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Consumer '{consumerUsername}' created.");
            }
            else
            {
                Console.WriteLine($"Consumer '{consumerUsername}' already exists.");
            }

            var jwtCredentialResponse = await client.GetAsync($"{kongAdminUrl}/consumers/{consumerUsername}/jwt");
            var jwtCredentialContent = await jwtCredentialResponse.Content.ReadAsStringAsync();
            dynamic jwtCredentials = JsonConvert.DeserializeObject(jwtCredentialContent);

            bool jwtExists = false;
            foreach (var jwtCredential in jwtCredentials.data)
            {
                if (jwtCredential.key == "jens-key")
                {
                    jwtExists = true;
                    Console.WriteLine("JWT credential already exists for 'jens-key'.");
                    break;
                }
            }
            if (!jwtExists)
            {
                var jwtCredentialData = new
                {
                    key = "jens-key",
                    secret = "PelleDanmarksStatsministerI2024ogdetskalbareskenu"
                };

                var jwtCredentialContentToAdd = new StringContent(JsonConvert.SerializeObject(jwtCredentialData), Encoding.UTF8, "application/json");
                var createJwtCredentialResponse = await client.PostAsync($"{kongAdminUrl}/consumers/{consumerUsername}/jwt", jwtCredentialContentToAdd);
                createJwtCredentialResponse.EnsureSuccessStatusCode();
                Console.WriteLine("JWT credential created for consumer 'jens'.");
            }
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
        }
    }
}
