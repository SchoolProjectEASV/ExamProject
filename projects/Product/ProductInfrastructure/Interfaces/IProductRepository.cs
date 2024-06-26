﻿using Domain.MongoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductInfrastructure.Interfaces
{
    /// <summary>
    /// Interface containing the methods implemented in ProductRepository
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// Adds a product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<bool> AddProduct(Product product);
        /// <summary>
        /// Gets all products
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Product>> GetAllProducts();
        /// <summary>
        /// Gets a product by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Product> GetProductById(string id);
        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        Task<bool> UpdateProduct(string id, Product product);
        /// <summary>
        /// Deletes a product
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteProduct(string id);
    }
}
