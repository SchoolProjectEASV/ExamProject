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


namespace ProductService.UnitTest;

public class ProductServiceUnitTest
{
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly ProductApplication.ProductService _productService;
    private readonly Mock<IConnectionMultiplexer> _mockRedis;

    public ProductServiceUnitTest()
    {
        _mockProductRepository = new Mock<IProductRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _mockRedis = new Mock<IConnectionMultiplexer>();

        var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

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
    /// Test to ensure that the AddProduct adds a new product successfully
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
        _mockProductRepository.Setup(repo => repo.AddProductAsync(product)).ReturnsAsync(true);

        // Act
        var result = await _productService.AddProductAsync(createProductDto);

        // Assert
        Assert.True(result);
        _mockMapper.Verify(m => m.Map<Product>(createProductDto), Times.Once);
        _mockProductRepository.Verify(repo => repo.AddProductAsync(product), Times.Once);
    }

    /// <summary>
    /// Test to ensure a product is deleted successfully
    /// </summary>
    [Fact]
    public async Task DeleteProductAsync_Success()
    {
        // Arrange
        var productId = ObjectId.GenerateNewId().ToString();
        var product = new Product { _id = new ObjectId(productId) };
        
        _mockProductRepository.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync(product);
        _mockProductRepository.Setup(repo => repo.DeleteProductAsync(productId)).ReturnsAsync(true);
        
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
        
        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        Assert.True(result);
        _mockProductRepository.Verify(repo => repo.GetProductByIdAsync(productId), Times.Once);
        _mockProductRepository.Verify(repo => repo.DeleteProductAsync(productId), Times.Once);
    }

    /// <summary>
    /// Tests the getAllProducts success scenario
    /// </summary>
    [Fact]
    public async Task GetAllProductsAsync_Success()
    {
        // Arrange
        {
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

            _mockProductRepository.Setup(repo => repo.GetAllProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _mockProductRepository.Verify(repo => repo.GetAllProductsAsync(), Times.Once);
            
        }
    }
    
    /// <summary>
    /// Tests the getProductById success scenario
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task GetProductByIdAsync_Success()
    {
        // Arrange
        var productId = ObjectId.GenerateNewId().ToString();
        var product = new Product 
            { 
                _id = new ObjectId(productId),
                Name = "Test Product", Description = "Test Description",
                Price = 1,
                Quantity = 2,
                CreatedAt = DateTime.UtcNow 
            };

        _mockProductRepository.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result._id.ToString());
        Assert.Equal("Test Product", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(1, result.Price);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(product.CreatedAt, result.CreatedAt);
        _mockProductRepository.Verify(repo => repo.GetProductByIdAsync(productId), Times.Once);
    }

    /// <summary>
    /// Tests to ensure the product is updated successfully
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task UpdateProductAsync_Success()
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

        _mockProductRepository.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync(existingProduct);
        _mockProductRepository.Setup(repo => repo.UpdateProductAsync(productId, existingProduct)).ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map(updateProductDto, existingProduct)).Callback<UpdateProductDTO, Product>((dto, product) =>
        {
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Quantity = dto.Quantity;
        });

        // Act
        var result = await _productService.UpdateProductAsync(productId, updateProductDto);

        // Assert
        Assert.True(result);
        _mockProductRepository.Verify(repo => repo.GetProductByIdAsync(productId), Times.Once);
        _mockProductRepository.Verify(repo => repo.UpdateProductAsync(productId, existingProduct), Times.Once);
        _mockMapper.Verify(m => m.Map(updateProductDto, existingProduct), Times.Once);
    }
    
    /// <summary>
    /// Tests update product failure
    /// </summary>
    [Fact]
    public async Task UpdateProductAsync_Fails()
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

        _mockProductRepository.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync((Product?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _productService.UpdateProductAsync(productId, updateProductDto));
        Assert.Equal($"Product with the id {productId} was not found", exception.Message);
    }

}

