using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
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

        public async Task<bool> AddProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteProduct(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                throw new ArgumentException($"Invalid product ID: {id}");
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p._id == objectId);

            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetProductById(string id)
        {
            var objectId = new ObjectId(id);
            var product = await _context.Products.FirstOrDefaultAsync(p => p._id == objectId);
            return product ?? throw new KeyNotFoundException($"No product could be found with the given id {id}");
        }

        public async Task<bool> UpdateProduct(string id, Product updatedProduct)
        {
            var productToUpdate = await GetProductById(id);
            if (productToUpdate == null)
            {
                throw new KeyNotFoundException($"No product found with the given ID: {id}");
            }

            productToUpdate.Name = updatedProduct.Name;
            productToUpdate.Description = updatedProduct.Description;
            productToUpdate.Price = updatedProduct.Price;
            productToUpdate.Quantity = updatedProduct.Quantity;

            _context.Products.Update(productToUpdate);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
