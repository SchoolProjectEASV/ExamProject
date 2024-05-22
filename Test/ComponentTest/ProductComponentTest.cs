using Microsoft.Extensions.Configuration;
using Moq;
using ProductApplication.Interfaces;
using WireMock.Server;

namespace ProductService.UnitTest.ComponentTest;

/// <summary>
/// Class containing the component test for the product service
/// </summary>
public class ProductComponentTest
{
    private WireMockServer _categoryServiceMock;
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<IProductService> _mockProductService;
}