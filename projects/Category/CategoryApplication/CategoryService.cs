using AutoMapper;
using CategoryApplication.DTO;
using CategoryApplication.Interfaces;
using CategoryInfrastructure.Interfaces;
using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;

namespace CategoryApplication
{
    /// <summary>
    /// Class containing the logic for the category crud operations
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<bool> AddCategoryAsync(CreateCategoryDTO createCategoryDTO)
        {
            var category = _mapper.Map<Category>(createCategoryDTO);
            category.CreatedAt = DateTime.UtcNow;

            return await _categoryRepository.AddCategoryAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            return await _categoryRepository.DeleteCategoryAsync(id);

        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(string id)
        {
            return await _categoryRepository.GetCategoryByIdAsync(id);
        }

        public async Task<bool> UpdateCategoryAsync(string id, UpdateCategoryDTO updateCategoryDTO)
        {
            var updatedCategory = _mapper.Map<Category>(updateCategoryDTO);
            updatedCategory._id = new MongoDB.Bson.ObjectId(id);

            return await _categoryRepository.UpdateCategoryAsync(id, updatedCategory);
        }
    }
}
