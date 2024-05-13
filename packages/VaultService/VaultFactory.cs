using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using VaultService;
using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.AppRole;
using VaultSharp.V1.AuthMethods.UserPass;

public class VauiltFactory: IVaultFactory
{
    private IVaultClient vaultClient;
    private readonly VaultSettings _vaultSettings;

    public VauiltFactory(IOptions<VaultSettings> settings )
    {
        _vaultSettings = settings.Value;
        IAuthMethodInfo authMethod = new AppRoleAuthMethodInfo(_vaultSettings.AppRole.RoleId, _vaultSettings.AppRole.SecretId);
        var vaultClientSettings = new VaultClientSettings(_vaultSettings.Address, authMethod);
        vaultClient = new VaultClient(vaultClientSettings);
    }

    public async Task<string> GetSecretAsync(string path, string key)
    {
        var secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(path);
        return secret.Data.Data.TryGetValue(key, out object? value) ? value?.ToString() : null;
    }
}
