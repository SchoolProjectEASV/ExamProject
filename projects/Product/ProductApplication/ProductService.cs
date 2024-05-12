using AutoMapper;
using Domain.MongoEntities;
using ProductApplication.DTO;
using ProductApplication.Interfaces;
using ProductInfrastructure.Interfaces;

namespace ProductApplication
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;


        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<bool> AddProductAsync(CreateProductDTO createProductDTO)
        {
            var product = _mapper.Map<Product>(createProductDTO);
            product.CreatedAt = DateTime.UtcNow;

            return await _productRepository.AddProductAsync(product);
        }

        public async Task<bool> DeleteProductAsync(string id)
        {
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
