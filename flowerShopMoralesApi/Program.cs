using flowerShopMoralesApi.Infrastructure.Persistence;
using flowerShopMoralesApi.Application.Interfaces;
using flowerShopMoralesApi.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Google.Cloud.SecretManager.V1;

var builder = WebApplication.CreateBuilder(args);

string LoadSecret(string secretId)
{
    var client = SecretManagerServiceClient.Create();
    var name = new SecretVersionName("flowershop-morales", secretId, "latest");
    var result = client.AccessSecretVersion(name);
    return result.Payload?.Data?.ToStringUtf8() ?? throw new InvalidOperationException($"Secret {secretId} not found or is null.");
}

string connectionString;

if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection string is not defined in the configuration.");
}
else
{
    connectionString = LoadSecret("db-connection-string");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITranslationService, OpenAiTranslationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FlowerShop API",
        Version = "v1"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => { });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlowerShop API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();