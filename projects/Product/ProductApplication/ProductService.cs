using Amazon.Runtime;
using AutoMapper;
using Domain.MongoEntities;
using Microsoft.Extensions.Configuration;
using ProductApplication.DTO;
using ProductApplication.Interfaces;
using ProductInfrastructure.Interfaces;
using System.Net.Http;

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

        public ProductService(IProductRepository productRepository, IMapper mapper, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _httpClient = httpClientFactory.CreateClient();
            _categoryServiceUrl = configuration["CategoryService:Url"];
            _httpClient.BaseAddress = new Uri(_categoryServiceUrl);
        }

        public async Task<bool> AddProductAsync(CreateProductDTO createProductDTO)
        {
            var product = _mapper.Map<Product>(createProductDTO);
            product.CreatedAt = DateTime.UtcNow;

            return await _productRepository.AddProductAsync(product);
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
            var product = await _productRepository.GetProductByIdAsync(id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with the id {id} was not found");
            }

            var response = await _httpClient.DeleteAsync($"/Category/removeProduct/{id}");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to remove product with id {id}");
            }

            return await _productRepository.DeleteProductAsync(id);
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllProductsAsync();
        }

        public async Task<Product> GetProductByIdAsync(string id)
        {
            return await _productRepository.GetProductByIdAsync(id);
        }

        public async Task<bool> UpdateProductAsync(string id, UpdateProductDTO updateProductDTO)
        {
            var updatedProduct = _mapper.Map<Product>(updateProductDTO);
            updatedProduct._id = new MongoDB.Bson.ObjectId(id);

            return await _productRepository.UpdateProductAsync(id, updatedProduct);
        }

    }
}
