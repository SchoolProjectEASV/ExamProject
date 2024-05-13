
public interface IVaultFactory
{
    Task<string?> GetSecretAsync(string path, string key);
}