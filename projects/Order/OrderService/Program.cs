using AutoMapper;
using OrderApplication.DTO;
using OrderApplication.Interfaces;
using OrderInfrastructure;
using OrderInfrastructure.Interfaces;
using VaultService;

var builder = WebApplication.CreateBuilder(args);

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
    config.CreateMap<AddOrderDTO, Domain.Order>();
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

app.MapControllers();

app.Run();
