using AutoMapper;
using Domain.HelperEntities;
using Domain.PostgressEntities;
using OpenTelemetry.Trace;
using OrderApplication.DTO;
using OrderApplication.Interfaces;
using OrderInfrastructure;
using OrderInfrastructure.Interfaces;
using Serilog;
using StackExchange.Redis;
using TracingService;
using VaultService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<VaultSettings>(builder.Configuration.GetSection("Vault"));

#region Dependency Injection
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderApplication.OrderService>();
builder.Services.AddScoped<IVaultFactory, VaultFactory>();
#endregion

#region AutoMapper
var mapper = new MapperConfiguration(config =>
{
    config.CreateMap<AddOrderDTO, Domain.PostgressEntities.Order>();
    config.CreateMap<UpdateOrderDTO, Domain.PostgressEntities.Order>();
    config.CreateMap<OrderProductDTO, OrderProduct>();
}).CreateMapper();
builder.Services.AddSingleton(mapper);
#endregion

#region Logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Seq("http://seq:5341")
    .WriteTo.Console()
    .CreateLogger();
#endregion

#region OpenTelemetry
builder.Services.AddOpenTelemetry().Setup("OrderService");
builder.Services.AddSingleton(TracerProvider.Default.GetTracer("OrderService"));
#endregion

#region Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetSection("Redis:Configuration").Value, true);
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});
#endregion

#region HttpClient
builder.Services.AddHttpClient();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
