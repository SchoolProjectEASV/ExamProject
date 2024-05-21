using CategoryInfrastructure.Interfaces;
using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace CategoryInfrastructure
{
    /// <summary>
    /// Category repository responsible for the communication with the category database
    /// </summary>
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CategoryDbContext _context;

        public CategoryRepository(CategoryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                throw new ArgumentException($"Invalid category ID: {id}");
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c._id == objectId);

            if (category == null)
            {
                return false;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;

        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(string id)
        {
            var objectId = new ObjectId(id);
            var category = await _context.Categories.FirstOrDefaultAsync(c => c._id == objectId);
            return category ?? throw new KeyNotFoundException($"No product could be found with the given id {id}");
        }

        public async Task<bool> UpdateCategoryAsync(string id, Category updatedCategory)
        {
            var categoryToUpdate = await GetCategoryByIdAsync(id);
            if (categoryToUpdate == null)
            {
                throw new KeyNotFoundException($"No category found with the given ID: {id}");
            }

            categoryToUpdate.Name = updatedCategory.Name;
            categoryToUpdate.Description = updatedCategory.Description;

            _context.Categories.Update(categoryToUpdate);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> AddProductToCategory(string categoryId, string productId)
        {
            var objectId = new ObjectId(categoryId);
            var category = await _context.Categories.FindAsync(objectId);
            if (category == null)
                return false;

            var productObjectId = new ObjectId(productId); 
            if (!category.ProductIds.Contains(productObjectId))
            {
                category.ProductIds.Add(productObjectId);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveProductFromCategory(string categoryId, string productId)
        {
            var objectId = new ObjectId(categoryId);
            var category = await _context.Categories.FindAsync(objectId);
            if (category == null)
                return false;

            var productObjectId = new ObjectId(productId); 
            if (category.ProductIds.Remove(productObjectId))
            {
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
