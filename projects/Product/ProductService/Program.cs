using AutoMapper;
using ProductApplication.DTO;
using ProductApplication.Interfaces;
using ProductInfrastructure;
using ProductInfrastructure.Interfaces;
using VaultService;
using TracingService;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<VaultSettings>(builder.Configuration.GetSection("Vault"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>();

#region DI
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductApplication.ProductService>();
builder.Services.AddScoped<IVaultFactory, VaultFactory>();
#endregion

#region AutoMapper
var mapper = new MapperConfiguration(config =>
{
    config.CreateMap<CreateProductDTO, Domain.MongoEntities.Product>();
    config.CreateMap<UpdateProductDTO, Domain.MongoEntities.Product>();
}).CreateMapper();
builder.Services.AddSingleton(mapper);
#endregion

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://seq:5341")
    .WriteTo.Console()
    .CreateLogger();

#region
builder.Services.AddOpenTelemetry().Setup("ProductService");
builder.Services.AddSingleton(TracerProvider.Default.GetTracer("ProductService"));
#endregion

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetSection("Redis:Configuration").Value, true);
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});
#region httpclient
builder.Services.AddHttpClient();
#endregion

builder.Services.AddLogging(logBuilder =>
{
    logBuilder.AddSeq("http://seq:5341");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();

app.Run();
