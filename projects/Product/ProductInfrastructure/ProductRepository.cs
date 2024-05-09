using Domain.MongoEntities;
using MongoClient;
using MongoDB.Driver;
using ProductInfrastructure.Interfaces;

namespace ProductInfrastructure
{
    public class ProductRepository : IProductRepository
    {
        private readonly IMongoCollection<Product> _productCollection;

        public ProductRepository(ProductDbContext dbContext) 
        {
            _productCollection = dbContext.GetCollection<Product>("products");
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            await _productCollection.InsertOneAsync(product);
            return product; 
        }
    }
}
