using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using ProductApplication.Interfaces;
using ProductInfrastructure.Interfaces;
using StackExchange.Redis;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

namespace ProductService.UnitTest.ComponentTest;

/// <summary>
/// Class containing the component test for the product service
/// </summary>
public class ProductComponentTest : IAsyncLifetime
{
    private WireMockServer _categoryServiceMock;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IProductRepository> _mockProductRepo;
    private HttpClient _httpClient;
    private IConnectionMultiplexer _redis;
    private ProductApplication.ProductService _productService;
    private Mock<IHttpClientFactory> _httpClientFactory;
    private Mock<IMapper> _mockMapper;
    private Mock<IDatabase> _mockDatabase;

    public async Task InitializeAsync()
    {
        _categoryServiceMock = WireMockServer.Start(new WireMockServerSettings
        {
            Urls = new[] { "http://localhost:8080" }
        });

        _mockProductRepo = new Mock<IProductRepository>();

        _httpClientFactory = new Mock<IHttpClientFactory>();

        var redisMock = new Mock<IConnectionMultiplexer>();
        _mockDatabase = new Mock<IDatabase>();


        _mockDatabase.Setup(db => db.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)string.Empty);

        _mockDatabase.Setup(db => db.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(_mockDatabase.Object);

        _redis = redisMock.Object;

        _httpClient = new HttpClient { BaseAddress = new Uri(_categoryServiceMock.Urls[0]) };
        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);
        _mockMapper = new Mock<IMapper>();

        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(config => config["CategoryService:Url"]).Returns(_categoryServiceMock.Urls[0]);

        _productService = new ProductApplication.ProductService(
            _mockProductRepo.Object,
            _mockMapper.Object,
            _httpClientFactory.Object,
            _mockConfiguration.Object,
            _redis);
    }

    public async Task DisposeAsync()
    {
        _categoryServiceMock.Stop();
    }
    
    /// <summary>
    /// Tests the successful deletion of a product in a scenario where categoryservice responds with a 200
    /// </summary>
    [Fact]
    public async Task DeleteProductAsync_Success()
    {
        // Arrange
        var productId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var product = new Domain.MongoEntities.Product { _id = new MongoDB.Bson.ObjectId(productId) };
            
        _mockProductRepo.Setup(repo => repo.GetProductById(productId)).ReturnsAsync(product);
        _mockProductRepo.Setup(repo => repo.DeleteProduct(productId)).ReturnsAsync(true);
            
        _categoryServiceMock.Given(Request.Create().WithPath($"/Category/removeProduct/{productId}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200));

        // Act
        var result = await _productService.DeleteProduct(productId);

        // Assert
        Assert.True(result);
        _mockProductRepo.Verify(repo => repo.GetProductById(productId), Times.Once);
        _mockProductRepo.Verify(repo => repo.DeleteProduct(productId), Times.Once);
    }
    
    /// <summary>
    /// Tests the scenario where the category service responds with a 404 error
    /// </summary>
    [Fact]
    public async Task DeleteProductAsync_Failure()
    {
        // Arrange
        var productId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var product = new Domain.MongoEntities.Product { _id = new MongoDB.Bson.ObjectId(productId) };
            
        _mockProductRepo.Setup(repo => repo.GetProductById(productId)).ReturnsAsync(product);
            
        _categoryServiceMock.Given(Request.Create().WithPath($"/Category/removeProduct/{productId}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(404));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _productService.DeleteProduct(productId));
        Assert.Equal($"Failed to remove product with id {productId}", exception.Message);
            
        _mockProductRepo.Verify(repo => repo.GetProductById(productId), Times.Once);
        _mockProductRepo.Verify(repo => repo.DeleteProduct(productId), Times.Never);
    }

}