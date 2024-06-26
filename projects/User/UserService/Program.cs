using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Trace;
using Serilog;
using StackExchange.Redis;
using System.Text;
using TracingService;
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
    config.CreateMap<Domain.PostgressEntities.User, GetUserDTO>();
    config.CreateMap<GetUserDTO, Domain.PostgressEntities.User>();
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

#region
builder.Services.AddOpenTelemetry().Setup("UserService");
builder.Services.AddSingleton(TracerProvider.Default.GetTracer("UserService"));
#endregion


#region httpclient
builder.Services.AddHttpClient();
#endregion

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration.GetSection("Redis:Configuration").Value, true);
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

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
