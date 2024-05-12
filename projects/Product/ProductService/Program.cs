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
using TracingService;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>();

#region DI
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductApplication.ProductService>();
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


app.UseAuthorization();

app.MapControllers();

app.Run();
