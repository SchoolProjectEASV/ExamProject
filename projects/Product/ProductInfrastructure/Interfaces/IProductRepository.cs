using Domain.MongoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductInfrastructure.Interfaces
{
    public interface IProductRepository
    {
        /// <summary>
        /// Adds a new product to the database
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<Product> AddProductAsync(Product product);

        /// <summary>
        /// Gets all the products from the database
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Product>> GetAllProductsAsync();

        /// <summary>
        /// Gets the product by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Product> GetProductByIdAsync(string id);

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<Product> UpdateProductAsync(string id, Product product);

        /// <summary>
        /// Deletes the product from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Product> DeleteProductAsync(string id);
    }
}
