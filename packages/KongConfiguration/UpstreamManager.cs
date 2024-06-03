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
