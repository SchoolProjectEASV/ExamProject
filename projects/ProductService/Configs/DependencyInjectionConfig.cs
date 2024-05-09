using ProductService.Core.Repositories;
using ProductService.Core.Repositories.Interfaces;
using ProductService.Core.Services;
using ProductService.Core.Services.Interfaces;

namespace ProductService.Configs;


    public static class DependencyInjectionConfig
    {
        public static void ConfigureDependencyInjection(this IServiceCollection services)
        {
        // Dependency injection
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, Core.Services.ProductService>();
        }


}
