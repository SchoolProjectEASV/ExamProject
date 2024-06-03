using System;
using System.Threading.Tasks;
using KongSetup.KongEntities;
using Newtonsoft.Json;

namespace KongSetup
{
    public class UpstreamManager
    {
        private readonly KongClient _kongClient;

        public UpstreamManager(KongClient kongClient)
        {
            _kongClient = kongClient;
        }

        /// <summary>
        /// Adds or updates an upstream in Kong.
        /// If the upstream already exists, it logs a message indicating this.
        /// Otherwise, it creates a new upstream with the provided name.
        /// </summary>
        /// <param name="upstreamName">The name of the upstream to add or update.</param>
        public async Task AddOrUpdateUpstream(string upstreamName)
        {
            var upstreamData = new { name = upstreamName };

            var upstreamResponse = await _kongClient.GetAsync($"upstreams/{upstreamName}");
            if (upstreamResponse.IsSuccessStatusCode)
            {
                Console.WriteLine($"Upstream {upstreamName} already exists.");
            }
            else
            {
                var createResponse = await _kongClient.PostAsync("upstreams", upstreamData);
                createResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Upstream {upstreamName} created.");
            }
        }

        /// <summary>
        /// Adds a target to a specified upstream in Kong.
        /// If the target already exists in the upstream, it logs a message indicating this.
        /// Otherwise, it adds the new target to the upstream.
        /// This is to create different targets to enable loadbalancing in Kong.
        /// </summary>
        /// <param name="upstreamName">The name of the upstream to which the target will be added.</param>
        /// <param name="target">The target to add to the upstream.</param>
        public async Task AddTargetToUpstream(string upstreamName, string target)
        {
            var targetResponse = await _kongClient.GetAsync($"upstreams/{upstreamName}/targets");
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

            var targetData = new { target = target };
            var createResponse = await _kongClient.PostAsync($"upstreams/{upstreamName}/targets", targetData);
            createResponse.EnsureSuccessStatusCode();
            Console.WriteLine($"Target {target} added to upstream {upstreamName}.");
        }

        /// <summary>
        /// Adds or updates a service in Kong.
        /// If the service already exists, it updates the service with the new upstream information.
        /// Otherwise, it creates a new service with the provided details.
        /// </summary>
        /// <param name="name">The name of the service to add or update.</param>
        /// <param name="upstreamName">The upstream name to associate with the service.</param>
        public async Task AddOrUpdateService(string name, string upstreamName)
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

            var serviceResponse = await _kongClient.GetAsync($"services/{name}");
            if (serviceResponse.IsSuccessStatusCode)
            {
                var updateContent = new { host = upstreamName };
                var updateResponse = await _kongClient.PatchAsync($"services/{name}", updateContent);
                updateResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Service {name} updated.");
            }
            else
            {
                var createResponse = await _kongClient.PostAsync("services", serviceData);
                createResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Service {name} added.");
            }
        }
    }
}
