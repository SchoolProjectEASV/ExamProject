using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Vault;
using Vault.Client;
using Vault.Model;
using VaultService;

public class VaultFactory : IVaultFactory
{
    private VaultClient vaultClient;
    private  VaultSettings _vaultSettings;

    public VaultFactory(IOptions<VaultSettings> settings)
    {
        _vaultSettings = settings.Value;
        VaultConfiguration vaultConfiguration = new VaultConfiguration(_vaultSettings.Address);
        vaultClient = new VaultClient(vaultConfiguration);
        var auth = vaultClient.Auth.UserpassLogin(_vaultSettings.UserPass.Username, new UserpassLoginRequest(_vaultSettings.UserPass.Password));
        vaultClient.SetToken(auth.ResponseAuth.ClientToken);
    }

    public async Task<string> GetConnectionStringCategory()
    {
        VaultResponse<KvV2ReadResponse> response = vaultClient.Secrets.KvV2Read("secret", "connectionstring");
        
        JObject data = (JObject)response.Data.Data;

        _vaultSettings = data.ToObject<VaultSettings>();

        return _vaultSettings.CONNECTIONSTRING_MONGODB;

    }

    public async Task<string> GetConnectionStringProduct()
    {
        VaultResponse<KvV2ReadResponse> response = vaultClient.Secrets.KvV2Read("secret", "connectionstring");

        JObject data = (JObject) response.Data.Data;

        _vaultSettings = data.ToObject<VaultSettings>();

        return _vaultSettings.CONNECTIONSTRING_MONGODB;
    }

    public async Task<string> GetConnectionStringUser()
    {
        VaultResponse<KvV2ReadResponse> response = vaultClient.Secrets.KvV2Read("secretUser", "connectionstring");

        JObject data = (JObject) response.Data.Data;

        _vaultSettings = data.ToObject<VaultSettings>();

        return _vaultSettings.CONNECTIONSTRING_USER_POSTGRESS;
    }
}
