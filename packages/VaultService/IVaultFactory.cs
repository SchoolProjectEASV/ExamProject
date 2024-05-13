
public interface IVaultFactory
{
    public Task<string> GetConnectionStringProduct();

    public Task<string> GetConnectionStringCategory();
}