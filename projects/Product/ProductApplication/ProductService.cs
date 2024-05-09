using ProductApplication.Interfaces;
using ProductInfrastructure.Interfaces;

namespace ProductApplication
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;


        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
    }
}
