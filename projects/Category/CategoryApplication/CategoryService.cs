using AutoMapper;
using CategoryApplication.DTO;
using CategoryApplication.Interfaces;
using CategoryInfrastructure.Interfaces;
using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace CategoryApplication
{
    /// <summary>
    /// Class containing the logic for the category crud operations
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly string? _productServiceUrl;
        private readonly HttpClient _httpClient;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _productServiceUrl = configuration["ProductService:Url"];
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_productServiceUrl);
        }


        public async Task AddProductToCategory(string categoryId, string productId)
        {
            var response = await _httpClient.GetAsync($"/Product/{productId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"Product with the id {productId} was not found.");
            }

            await _categoryRepository.AddProductToCategory(categoryId, productId);
        }

        public async Task RemoveProductFromCategory(string categoryId, string productId)
        {
            var removedSuccessfully = await _categoryRepository.RemoveProductFromCategory(categoryId, productId);
            if (!removedSuccessfully)
            {
                throw new KeyNotFoundException($"Product with the id {productId} was not found in the category {categoryId}.");
            }
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

        public async Task RemoveProductFromAllCategories(string productId)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            foreach (var category in categories)
            {
                if (category.ProductIds.Contains(new MongoDB.Bson.ObjectId(productId)))
                {
                    await _categoryRepository.RemoveProductFromCategory(category._id.ToString(), productId);
                }
            }
        }
    }
}
