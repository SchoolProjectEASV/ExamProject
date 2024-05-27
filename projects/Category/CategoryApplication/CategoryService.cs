using AutoMapper;
using CategoryApplication.DTO;
using CategoryApplication.Interfaces;
using CategoryInfrastructure.Interfaces;
using Domain.MongoEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

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
        private readonly IConnectionMultiplexer _redis;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _productServiceUrl = configuration["ProductService:Url"];
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(_productServiceUrl);
            _redis = redis;
        }

        public async Task AddProductToCategory(string categoryId, string productId)
        {
            var response = await _httpClient.GetAsync($"/Product/{productId}");
            if (!response.IsSuccessStatusCode)
            {
                throw new KeyNotFoundException($"Product with the id {productId} was not found.");
            }

            await _categoryRepository.AddProductToCategory(categoryId, productId);
            await RemoveCachedCategoryAsync(categoryId);
        }

        public async Task RemoveProductFromCategory(string categoryId, string productId)
        {
            var removedSuccessfully = await _categoryRepository.RemoveProductFromCategory(categoryId, productId);
            if (!removedSuccessfully)
            {
                throw new KeyNotFoundException($"Product with the id {productId} was not found in the category {categoryId}.");
            }
            await RemoveCachedCategoryAsync(categoryId);
        }

        public async Task<bool> AddCategoryAsync(CreateCategoryDTO createCategoryDTO)
        {
            var category = _mapper.Map<Category>(createCategoryDTO);
            category.CreatedAt = DateTime.UtcNow;

            return await _categoryRepository.AddCategoryAsync(category);
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            var result = await _categoryRepository.DeleteCategoryAsync(id);
            if (result)
            {
                await RemoveCachedCategoryAsync(id);
            }
            return result;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllCategoriesAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(string id)
        {
            var cachedCategory = await GetCachedCategoryAsync(id);
            if (cachedCategory != null)
            {
                return cachedCategory;
            }

            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category != null)
            {
                await CacheCategoryAsync(category);
            }

            return category;
        }

        public async Task<bool> UpdateCategoryAsync(string id, UpdateCategoryDTO updateCategoryDTO)
        {
            var updatedCategory = _mapper.Map<Category>(updateCategoryDTO);
            updatedCategory._id = new MongoDB.Bson.ObjectId(id);

            var result = await _categoryRepository.UpdateCategoryAsync(id, updatedCategory);
            if (result)
            {
                await CacheCategoryAsync(updatedCategory);
            }

            return result;
        }

        public async Task RemoveProductFromAllCategories(string productId)
        {
            var categories = await _categoryRepository.GetAllCategoriesAsync();
            foreach (var category in categories)
            {
                if (category.ProductIds.Contains(new MongoDB.Bson.ObjectId(productId)))
                {
                    await _categoryRepository.RemoveProductFromCategory(category._id.ToString(), productId);
                    await RemoveCachedCategoryAsync(category._id.ToString());
                }
            }
        }

        private async Task CacheCategoryAsync(Category category)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(GetRedisKeyForCategory(category._id.ToString()), JsonSerializer.Serialize(category));
        }

        private async Task<Category> GetCachedCategoryAsync(string categoryId)
        {
            var db = _redis.GetDatabase();
            var categoryJson = await db.StringGetAsync(GetRedisKeyForCategory(categoryId));
            if (!string.IsNullOrEmpty(categoryJson))
            {
                return JsonSerializer.Deserialize<Category>(categoryJson);
            }
            return null;
        }

        private async Task RemoveCachedCategoryAsync(string categoryId)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(GetRedisKeyForCategory(categoryId));
        }

        private string GetRedisKeyForCategory(string categoryId)
        {
            return $"category:{categoryId}";
        }
    }
}
