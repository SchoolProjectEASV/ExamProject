using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using ProductApplication.Interfaces;
using ProductInfrastructure.Interfaces;
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
    private ProductApplication.ProductService _productService;
    private Mock<IHttpClientFactory> _httpClientFactory;
    private Mock<IMapper> _mockMapper;
    

    public async Task InitializeAsync()
    {
        _categoryServiceMock = WireMockServer.Start(new WireMockServerSettings
        {
            Urls = new[] { "http://localhost:8080" }
        });


        _mockProductRepo = new Mock<IProductRepository>();
            
        _httpClientFactory = new Mock<IHttpClientFactory>(); 
        _httpClient = new HttpClient { BaseAddress = new Uri(_categoryServiceMock.Urls[0]) };
        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);
        _mockMapper = new Mock<IMapper>();

        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(config => config["CategoryService:Url"]).Returns(_categoryServiceMock.Urls[0]);
        _productService = new ProductApplication.ProductService(_mockProductRepo.Object,  _mockMapper.Object, _httpClientFactory.Object, _mockConfiguration.Object);
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
            
        _mockProductRepo.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync(product);
        _mockProductRepo.Setup(repo => repo.DeleteProductAsync(productId)).ReturnsAsync(true);
            
        _categoryServiceMock.Given(Request.Create().WithPath($"/Category/removeProduct/{productId}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(200));

        // Act
        var result = await _productService.DeleteProductAsync(productId);

        // Assert
        Assert.True(result);
        _mockProductRepo.Verify(repo => repo.GetProductByIdAsync(productId), Times.Once);
        _mockProductRepo.Verify(repo => repo.DeleteProductAsync(productId), Times.Once);
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
            
        _mockProductRepo.Setup(repo => repo.GetProductByIdAsync(productId)).ReturnsAsync(product);
            
        _categoryServiceMock.Given(Request.Create().WithPath($"/Category/removeProduct/{productId}").UsingDelete())
            .RespondWith(Response.Create().WithStatusCode(404));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _productService.DeleteProductAsync(productId));
        Assert.Equal($"Failed to remove product with id {productId}", exception.Message);
            
        _mockProductRepo.Verify(repo => repo.GetProductByIdAsync(productId), Times.Once);
        _mockProductRepo.Verify(repo => repo.DeleteProductAsync(productId), Times.Never);
    }

}