using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KongSetup
{
    public class KongClient
    {
        private readonly HttpClient _client;
        private readonly string _kongAdminUrl;

        public KongClient(string kongAdminUrl)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            _client = new HttpClient(handler);
            _kongAdminUrl = kongAdminUrl;
        }

        public async Task<HttpResponseMessage> GetAsync(string path) =>
            await _client.GetAsync($"{_kongAdminUrl}/{path}");

        public async Task<HttpResponseMessage> PostAsync(string path, object data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _client.PostAsync($"{_kongAdminUrl}/{path}", content);
        }

        public async Task<HttpResponseMessage> PatchAsync(string path, object data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _client.PatchAsync($"{_kongAdminUrl}/{path}", content);
        }
    }
}
