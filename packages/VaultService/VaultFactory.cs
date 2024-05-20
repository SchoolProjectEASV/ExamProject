using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Vault;
using Vault.Client;
using Vault.Model;
using VaultService;
using Polly;
using Polly.Retry;

public class VaultFactory : IVaultFactory
{
    private VaultClient vaultClient;
    private VaultSettings _vaultSettings;
    private readonly AsyncRetryPolicy _retryPolicy;

    public VaultFactory(IOptions<VaultSettings> settings)
    {
        _vaultSettings = settings.Value;
        VaultConfiguration vaultConfiguration = new VaultConfiguration(_vaultSettings.Address);
        vaultClient = new VaultClient(vaultConfiguration);
        var auth = vaultClient.Auth.UserpassLogin(_vaultSettings.UserPass.Username, new UserpassLoginRequest(_vaultSettings.UserPass.Password));
        vaultClient.SetToken(auth.ResponseAuth.ClientToken);
        _retryPolicy = PollyPolicy.GetRetryPolicy();
    }

    public string GetConnectionStringCategory()
    {
        return _retryPolicy.ExecuteAsync(async () =>
        {
            VaultResponse<KvV2ReadResponse> response = await vaultClient.Secrets.KvV2ReadAsync("secret", "connectionstring");
            JObject data = (JObject)response.Data.Data;
            _vaultSettings = data.ToObject<VaultSettings>();
            return _vaultSettings.CONNECTIONSTRING_MONGODB;
        }).GetAwaiter().GetResult();
    }

    public string GetConnectionStringProduct()
    {
        return _retryPolicy.ExecuteAsync(async () =>
        {
            VaultResponse<KvV2ReadResponse> response = await vaultClient.Secrets.KvV2ReadAsync("secret", "connectionstring");
            JObject data = (JObject)response.Data.Data;
            _vaultSettings = data.ToObject<VaultSettings>();
            return _vaultSettings.CONNECTIONSTRING_MONGODB;
        }).GetAwaiter().GetResult();
    }

    public string GetConnectionStringUser()
    {
        return _retryPolicy.ExecuteAsync(async () =>
        {
            VaultResponse<KvV2ReadResponse> response = await vaultClient.Secrets.KvV2ReadAsync("secretUser", "connectionstring");
            JObject data = (JObject)response.Data.Data;
            _vaultSettings = data.ToObject<VaultSettings>();
            return _vaultSettings.CONNECTIONSTRING_USER_POSTGRESS;
        }).GetAwaiter().GetResult();
    }

    public string GetConnectionStringOrder()
    {
        return _retryPolicy.ExecuteAsync(async () =>
        {
            VaultResponse<KvV2ReadResponse> response = await vaultClient.Secrets.KvV2ReadAsync("secretOrder", "connectionstring");
            JObject data = (JObject)response.Data.Data;
            _vaultSettings = data.ToObject<VaultSettings>();
            return _vaultSettings.CONNECTIONSTRING_ORDER_POSTGRESS;
        }).GetAwaiter().GetResult();
    }
}
