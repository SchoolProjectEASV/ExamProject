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

        public async Task<Product> AddProductAsync(CreateProductDTO createProductDTO)
        {
            var product = _mapper.Map<Product>(createProductDTO);
            product.CreatedAt = DateTime.UtcNow;

            return await _productRepository.AddProductAsync(product);
        }
    }
}
