using Domain.MongoEntities;
using ProductApplication.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApplication.Interfaces
{
    /// <summary>
    /// Interface containing the methods implemented in ProductService
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Add a product
        /// </summary>
        /// <param name="createProductDTO"></param>
        /// <returns></returns>
        Task<bool> AddProduct(CreateProductDTO createProductDTO);
        /// <summary>
        /// Gets all the products
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Product>> GetAllProducts();
        /// <summary>
        /// Gets a product by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Product> GetProductById(string id);
        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateProductDTO"></param>
        /// <returns></returns>
        Task<bool> UpdateProduct(string id, UpdateProductDTO updateProductDTO);
        /// <summary>
        /// Deletes a product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteProduct(string id);

    }
}
