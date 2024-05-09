using ProductInfrastructure.Interfaces;

namespace ProductInfrastructure
{
    public class ProductRepository : IProductRepository
    {
        private readonly Client _client;
        private readonly string _databaseName;
        private readonly string _collectionName;

        public ProductRepository() { }

    }
}
