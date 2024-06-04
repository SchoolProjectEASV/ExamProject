using AutoMapper;
using Domain.MongoEntities;
using Microsoft.Extensions.Configuration;
using ProductApplication.DTO;
using ProductApplication.Interfaces;
using ProductInfrastructure.Interfaces;
using StackExchange.Redis;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductApplication
{
    /// <summary>
    /// Class containing the logic for the product crud operations
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly string _categoryServiceUrl;
        private readonly HttpClient _httpClient;
        private readonly IConnectionMultiplexer _redis;

        public ProductService(IProductRepository productRepository, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration, IConnectionMultiplexer redis)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            _categoryServiceUrl = configuration["CategoryService:Url"];
            _httpClient.BaseAddress = new Uri(_categoryServiceUrl);
            _redis = redis;
        }

        public async Task<bool> AddProduct(CreateProductDTO createProductDTO)
        {
            var product = _mapper.Map<Product>(createProductDTO);
            product.CreatedAt = DateTime.UtcNow;

            var result = await _productRepository.AddProduct(product);
            if (result)
            {
                await CacheProductAsync(product);
            }

            return result;
        }


        public async Task<bool> DeleteProduct(string id)
        {
            var product = await _productRepository.GetProductById(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with the id {id} was not found");
            }

            var response = await _httpClient.DeleteAsync($"/Category/removeProduct/{id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to remove product with id {id}");
            }

            var result = await _productRepository.DeleteProduct(id);
            if (result)
            {
                await RemoveCachedProductAsync(id);
            }

            return result;
        }

        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _productRepository.GetAllProducts();
        }

        public async Task<Product> GetProductById(string id)
        {
            var cachedProduct = await GetCachedProductAsync(id);
            if (cachedProduct != null)
            {
                return cachedProduct;
            }

            var product = await _productRepository.GetProductById(id);
            if (product != null)
            {
                await CacheProductAsync(product);
            }

            return product;
        }

        public async Task<bool> UpdateProduct(string id, UpdateProductDTO updateProductDTO)
        {
            var updatedProduct = _mapper.Map<Product>(updateProductDTO);
            updatedProduct._id = new MongoDB.Bson.ObjectId(id);

            var result = await _productRepository.UpdateProduct(id, updatedProduct);
            if (result)
            {
                await CacheProductAsync(updatedProduct);
            }

            return result;
        }

        private async Task CacheProductAsync(Product product)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(GetRedisKeyForProduct(product._id.ToString()), JsonSerializer.Serialize(product));
        }

        private async Task<Product> GetCachedProductAsync(string productId)
        {
            var db = _redis.GetDatabase();
            var productJson = await db.StringGetAsync(GetRedisKeyForProduct(productId));
            if (!string.IsNullOrEmpty(productJson))
            {
                return JsonSerializer.Deserialize<Product>(productJson);
            }
            return null;
        }

        private async Task RemoveCachedProductAsync(string productId)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(GetRedisKeyForProduct(productId));
        }

        private string GetRedisKeyForProduct(string productId)
        {
            return $"product:{productId}";
        }
    }
}
