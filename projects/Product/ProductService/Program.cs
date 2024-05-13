using AutoMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductApplication;
using ProductApplication.DTO;
using ProductApplication.Interfaces;
using ProductInfrastructure;
using ProductInfrastructure.Interfaces;
using System.Diagnostics.Metrics;
using Domain;
using VaultService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<VaultSettings>(builder.Configuration.GetSection("Vault"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>();

#region
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductApplication.ProductService>();
builder.Services.AddScoped<IVaultFactory, VauiltFactory>();
#endregion

#region AutoMapper
var mapper = new MapperConfiguration(config =>
{
    config.CreateMap<CreateProductDTO, Domain.MongoEntities.Product>();
    config.CreateMap<UpdateProductDTO, Domain.MongoEntities.Product>();
}).CreateMapper();
builder.Services.AddSingleton(mapper);
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
