using Domain.MongoEntities;
using MongoDB.Driver;
using ProductInfrastructure.Interfaces;

namespace ProductInfrastructure
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;

        public ProductRepository(ProductDbContext dbContext) 
        {
            _context = dbContext;
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
            return product;
        }
    }
}
