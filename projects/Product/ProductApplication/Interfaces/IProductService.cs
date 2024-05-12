using Domain.MongoEntities;
using ProductApplication.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApplication.Interfaces
{
    public interface IProductService
    {
        /// <summary>
        /// Add a product
        /// </summary>
        /// <param name="createProductDTO"></param>
        /// <returns></returns>
        Task<bool> AddProductAsync(CreateProductDTO createProductDTO);
        /// <summary>
        /// Gets all the products
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Product>> GetAllProductsAsync();
        /// <summary>
        /// Gets a product by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Product> GetProductByIdAsync(string id);
        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateProductDTO"></param>
        /// <returns></returns>
        Task<bool> UpdateProductAsync(string id, UpdateProductDTO updateProductDTO);
        /// <summary>
        /// Deletes a product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteProductAsync(string id);

    }
}
