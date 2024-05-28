using AuthApplication;
using VaultService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<VaultSettings>(builder.Configuration.GetSection("Vault"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


#region Dependency Injection

builder.Services.AddScoped<IAuthRepo, AuthRepo>();
builder.Services.AddScoped<IAuthService, AuthApplication.AuthService>();
builder.Services.AddScoped<IVaultFactory, VaultFactory>();
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
