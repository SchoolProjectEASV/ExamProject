using System.Net;
using System.Text;
using AutoMapper;
using Domain.MongoEntities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using Moq;
using Moq.Protected;
using ProductApplication.DTO;
using ProductInfrastructure.Interfaces;
using StackExchange.Redis;
using Xunit;

namespace ProductService.UnitTest
{
    /// <summary>
    /// Contains the unit tests for product service
    /// </summary>
    public class ProductServiceUnitTest
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly ProductApplication.ProductService _productService;
        private readonly Mock<IConnectionMultiplexer> _mockRedis;
        private readonly Mock<IDatabase> _mockDatabase;

        public ProductServiceUnitTest()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            _mockRedis = new Mock<IConnectionMultiplexer>();
            _mockDatabase = new Mock<IDatabase>();

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("http://localhost")
            };

            _mockDatabase.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync((RedisValue)string.Empty);

            _mockDatabase.Setup(db => db.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            _mockDatabase.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            _mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);

            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            _mockConfiguration.Setup(c => c["CategoryService:Url"]).Returns("http://localhost");

            _productService = new ProductApplication.ProductService(
                _mockProductRepository.Object,
                _mockMapper.Object,
                _mockHttpClientFactory.Object,
                _mockConfiguration.Object,
                _mockRedis.Object
            );
        }

        /// <summary>
        /// Tests to ensure that the AddProduct method successfully adds a product
        /// </summary>
        [Fact]
        public async Task AddProductAsync_Success()
        {
            // Arrange
            var createProductDto = new CreateProductDTO
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 1,
                Quantity = 5
            };

            var product = new Product
            {
                _id = ObjectId.GenerateNewId(),
                Name = "Test Product",
                Description = "Test Description",
                Price = 1,
                Quantity = 10,
                CreatedAt = DateTime.UtcNow
            };

            _mockMapper.Setup(m => m.Map<Product>(createProductDto)).Returns(product);
            _mockProductRepository.Setup(repo => repo.AddProduct(product)).ReturnsAsync(true);

            // Act
            var result = await _productService.AddProduct(createProductDto);

            // Assert
            Assert.True(result);
            _mockMapper.Verify(m => m.Map<Product>(createProductDto), Times.Once);
            _mockProductRepository.Verify(repo => repo.AddProduct(product), Times.Once);
        }

        /// <summary>
        /// Testing successfull product deletion
        /// </summary>
        [Fact]
        public async Task DeleteProductAsync_Success()
        {
            // Arrange
            var productId = ObjectId.GenerateNewId().ToString();
            var product = new Product { _id = new ObjectId(productId) };

            _mockProductRepository.Setup(repo => repo.GetProductById(productId)).ReturnsAsync(product);
            _mockProductRepository.Setup(repo => repo.DeleteProduct(productId)).ReturnsAsync(true);

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("", Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _productService.DeleteProduct(productId);

            // Assert
            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.GetProductById(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.DeleteProduct(productId), Times.Once);
        }

        /// <summary>
        /// Gets all products successfully
        /// </summary>
        [Fact]
        public async Task GetAllProductsAsync_Success()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product
                {
                    _id = ObjectId.GenerateNewId(),
                    Name = "Test Product 1",
                    Description = "Description 1",
                    Price = 1,
                    Quantity = 2,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    _id = ObjectId.GenerateNewId(),
                    Name = "Test Product 2",
                    Description = "Description 2",
                    Price = 2,
                    Quantity = 2,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockProductRepository.Setup(repo => repo.GetAllProducts()).ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockProductRepository.Verify(repo => repo.GetAllProducts(), Times.Once);
        }

        /// <summary>
        /// Gets a product by its id successfully
        /// </summary>
        [Fact]
        public async Task GetProductByIdAsync_Success()
        {
            // Arrange
            var productId = ObjectId.GenerateNewId().ToString();
            var product = new Product
            {
                _id = new ObjectId(productId),
                Name = "Test Product",
                Description = "Test Description",
                Price = 1,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow
            };

            _mockProductRepository.Setup(repo => repo.GetProductById(productId)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductById(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result._id.ToString());
            Assert.Equal("Test Product", result.Name);
            Assert.Equal("Test Description", result.Description);
            Assert.Equal(1, result.Price);
            Assert.Equal(2, result.Quantity);
            Assert.Equal(product.CreatedAt, result.CreatedAt);
            _mockProductRepository.Verify(repo => repo.GetProductById(productId), Times.Once);
        }

        /// <summary>
        /// Tests that UpdateProduct successfully updates a product with new values 
        /// </summary>
        [Fact]
        public async Task UpdateProduct_Success()
        {
            // Arrange
            var productId = ObjectId.GenerateNewId().ToString();
            var updateProductDto = new UpdateProductDTO
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 1,
                Quantity = 2
            };

            var existingProduct = new Product
            {
                _id = new ObjectId(productId),
                Name = "Test Product",
                Description = "Test Description",
                Price = 1,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow
            };

            var updatedProduct = new Product
            {
                _id = new ObjectId(productId),
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 1,
                Quantity = 2,
                CreatedAt = existingProduct.CreatedAt
            };

            _mockProductRepository.Setup(repo => repo.GetProductById(productId)).ReturnsAsync(existingProduct);

            _mockProductRepository.Setup(repo => repo.UpdateProduct(productId, updatedProduct)).ReturnsAsync(true);

            _mockMapper.Setup(m => m.Map<Product>(updateProductDto)).Returns(updatedProduct);

            _mockDatabase.Setup(db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
                    It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // Act
            var result = await _productService.UpdateProduct(productId, updateProductDto);

            // Assert
            Assert.True(result);
            _mockProductRepository.Verify(repo => repo.UpdateProduct(productId, updatedProduct), Times.Once);
            _mockMapper.Verify(m => m.Map<Product>(updateProductDto), Times.Once);
            _mockDatabase.Verify(
                db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(),
                    It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Once);
        }
        
        /// <summary>
        /// Scenario where the update product fails
        /// </summary>
        [Fact]
        public async Task UpdateProductAsync_Failure()
        {
            // Arrange
            var productId = ObjectId.GenerateNewId().ToString();
            var updateProductDto = new UpdateProductDTO
            {
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 1,
                Quantity = 2
            };

            var existingProduct = new Product
            {
                _id = new ObjectId(productId),
                Name = "Test Product",
                Description = "Test Description",
                Price = 1,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow
            };

            var updatedProduct = new Product
            {
                _id = new ObjectId(productId),
                Name = "Updated Product",
                Description = "Updated Description",
                Price = 1,
                Quantity = 2,
                CreatedAt = existingProduct.CreatedAt
            };

            _mockProductRepository.Setup(repo => repo.GetProductById(productId)).ReturnsAsync(existingProduct);

            _mockProductRepository.Setup(repo => repo.UpdateProduct(productId, updatedProduct)).ReturnsAsync(false);

            _mockMapper.Setup(m => m.Map<Product>(updateProductDto)).Returns(updatedProduct);
            
            // Act
            var result = await _productService.UpdateProduct(productId, updateProductDto);

            // Assert
            Assert.False(result);
            _mockProductRepository.Verify(repo => repo.UpdateProduct(productId, updatedProduct), Times.Once);
            _mockMapper.Verify(m => m.Map<Product>(updateProductDto), Times.Once);
            _mockDatabase.Verify(
                db => db.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(),
                    It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()), Times.Never);
        }

    }
}
