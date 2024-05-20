using AutoMapper;
using Microsoft.Extensions.Configuration;
using UserApplication;
using UserApplication.DTO;
using UserInfrastructure;
using UserInfrastructure.Interfaces;
using VaultService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<VaultSettings>(builder.Configuration.GetSection("Vault"));


#region DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserApplication.UserService>();
builder.Services.AddScoped<IVaultFactory, VaultFactory>();
#endregion

#region AutoMapper

var mapper = new MapperConfiguration(config =>
{
    config.CreateMap<AddUserDTO, Domain.PostgressEntities.User>();
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
