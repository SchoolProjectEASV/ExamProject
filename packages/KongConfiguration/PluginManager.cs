using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KongSetup
{
    /// <summary>
    /// Class to manage the Kong plugins.
    /// </summary>
    public class PluginManager
    {
        private readonly KongClient _kongClient;
        private readonly KongSettings _kongSettings;

        public PluginManager(KongClient kongClient, KongSettings kongSettings)
        {
            _kongClient = kongClient;
            _kongSettings = kongSettings;
        }

        /// <summary>
        /// Enable global rate limiting plugin for all services.
        /// </summary>
        /// <returns></returns>
        public async Task EnableGlobalRateLimiting()
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

            var getResponse = await _kongClient.GetAsync("plugins");
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
            var response = await _kongClient.PostAsync("plugins", rateLimitingData);
            response.EnsureSuccessStatusCode();
            Console.WriteLine("Global rate limiting enabled.");
        }

        /// <summary>
        /// Adds a consumer and a JWT credential for the consumer. If the consumer already exist, it logs a message indicating this.
        /// It also checks if the JWT credential already exists for the consumer.
        /// And if it doesn't exist, it creates a new JWT credential for the consumer.
        /// </summary>
        /// <returns></returns>
        public async Task SetupJwtAuth()
        {
            var consumerUsername = "jens";

            var consumerResponse = await _kongClient.GetAsync($"consumers/{consumerUsername}");
            if (consumerResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var consumerData = new { username = consumerUsername };
                var createConsumerResponse = await _kongClient.PostAsync("consumers", consumerData);
                createConsumerResponse.EnsureSuccessStatusCode();
                Console.WriteLine($"Consumer '{consumerUsername}' created.");
            }
            else
            {
                Console.WriteLine($"Consumer '{consumerUsername}' already exists.");
            }

            var jwtCredentialResponse = await _kongClient.GetAsync($"consumers/{consumerUsername}/jwt");
            var jwtCredentialContent = await jwtCredentialResponse.Content.ReadAsStringAsync();
            dynamic jwtCredentials = JsonConvert.DeserializeObject(jwtCredentialContent);

            bool jwtExists = false;
            foreach (var jwtCredential in jwtCredentials.data)
            {
                if (jwtCredential.key == _kongSettings.key)
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
                    _kongSettings.key,
                    secret = _kongSettings.JwtSecret
                };
                var createJwtCredentialResponse = await _kongClient.PostAsync($"consumers/{consumerUsername}/jwt", jwtCredentialData);
                createJwtCredentialResponse.EnsureSuccessStatusCode();
                Console.WriteLine("JWT credential created for consumer 'jens'.");
            }
        }
    }
}
