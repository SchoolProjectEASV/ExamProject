using AutoMapper;
using CategoryApplication.DTO;
using CategoryApplication.Interfaces;
using CategoryInfrastructure;
using CategoryInfrastructure.Interfaces;
using OpenTelemetry.Trace;
using Serilog;
using TracingService;
using VaultService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<VaultSettings>(builder.Configuration.GetSection("Vault"));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CategoryDbContext>();

#region DI
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService, CategoryApplication.CategoryService>();
#endregion

#region Automapper 
var mapper = new MapperConfiguration(config =>
{
    config.CreateMap<CreateCategoryDTO, Domain.MongoEntities.Category>();
    config.CreateMap<UpdateCategoryDTO, Domain.MongoEntities.Category>();
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
builder.Services.AddOpenTelemetry().Setup("CategoryService");
builder.Services.AddSingleton(TracerProvider.Default.GetTracer("CategoryService"));
builder.Services.AddScoped<IVaultFactory, VaultFactory>();
#endregion

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

app.UseAuthorization();

app.MapControllers();

app.Run();
