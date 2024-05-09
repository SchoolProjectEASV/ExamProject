using AutoMapper;
using MongoClient;
using MongoDB.Bson;
using MongoDB.Driver;
using ProductApplication;
using ProductApplication.DTO;
using ProductApplication.Interfaces;
using ProductInfrastructure;
using ProductInfrastructure.Interfaces;
using System.Diagnostics.Metrics;
using Domain;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "placeholder";


// Add services to the container.

var client = new Client(connectionString);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(client);
builder.Services.AddScoped(provider => new ProductDbContext(client, "product"));

#region
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductApplication.ProductService>();
#endregion

#region AutoMapper
var mapper = new MapperConfiguration(config =>
{
    config.CreateMap<CreateProductDTO, Domain.MongoEntities.Product>();
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