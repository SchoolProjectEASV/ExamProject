using AutoMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using ProductApplication.Interfaces;
using ProductInfrastructure.Interfaces;
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
        _httpClient = new HttpClient { BaseAddress = new System.Uri(_categoryServiceMock.Urls[0]) };
        _httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);
        _mockMapper = new Mock<IMapper>();

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(config => config["PostService:Url"]).Returns(_categoryServiceMock.Urls[0]);
        _productService = new ProductApplication.ProductService(_mockProductRepo.Object,  _mockMapper.Object, _httpClientFactory.Object,
            mockConfiguration.Object);
    }

    public async Task DisposeAsync()
    {
        _categoryServiceMock.Stop();
    }

    [Fact]
    public async Task DeleteProductAsync_Success()
    {
        // Arrange
        
        // Act
        
        // Assert
    }

}