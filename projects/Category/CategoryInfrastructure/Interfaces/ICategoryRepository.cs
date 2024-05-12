using Domain.MongoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CategoryInfrastructure.Interfaces
{
    public interface ICategoryRepository
    {
        /// <summary>
        /// Adds a category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<bool> AddCategoryAsync(Category category);
        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        /// <summary>
        /// Gets a category by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Category> GetCategoryByIdAsync(string id);
        /// <summary>
        /// Updates the category
        /// </summary>
        /// <param name="id"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        Task<bool> UpdateCategoryAsync(string id, Category category);
        /// <summary>
        /// Deletes a category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> DeleteCategoryAsync(string id);

    }
}
