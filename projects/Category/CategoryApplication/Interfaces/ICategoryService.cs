using CategoryApplication.DTO;
using Domain.MongoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategoryApplication.Interfaces
{
    /// <summary>
    /// Interface containing the methods implemented in CategoryService
    /// </summary>
    public interface ICategoryService
    {
        /// <summary>
        /// Adds a category
        /// </summary>
        /// <param name="createCategoryDTO"></param>
        /// <returns></returns>
        Task<bool> AddCategoryAsync(CreateCategoryDTO createCategoryDTO);
        /// <summary>
        /// Gets all the categories
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        /// <summary>
        /// Gets a category by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Category> GetCategoryByIdAsync(string id);
        /// <summary>
        /// Updates the category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateCategoryDTO"></param>
        /// <returns></returns>
        Task<bool> UpdateCategoryAsync(string id, UpdateCategoryDTO updateCategoryDTO);
        /// <summary>
        /// Deletes the category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteCategoryAsync(string id);
        /// <summary>
        /// Adds a product to a category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task AddProductToCategory(string categoryId, string productId);
        /// <summary>
        /// Removes a product from a category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task RemoveProductFromCategory(string categoryId, string productId);
        /// <summary>
        /// Removes a product from any category it is added to
        /// Used when deleting a product to ensure it is deleted from the categories it is added to
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        Task RemoveProductFromAllCategories(string productId);


    }
}
