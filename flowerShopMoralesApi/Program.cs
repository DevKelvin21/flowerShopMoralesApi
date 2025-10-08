using flowerShopMoralesApi.Infrastructure.Persistence;
using flowerShopMoralesApi.Application.Interfaces;
using flowerShopMoralesApi.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Google.Cloud.SecretManager.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

string LoadSecret(string secretId)
{
    var client = SecretManagerServiceClient.Create();
    var name = new SecretVersionName("flowershop-morales", secretId, "latest");
    var result = client.AccessSecretVersion(name);
    return result.Payload?.Data?.ToStringUtf8() ?? throw new InvalidOperationException($"Secret {secretId} not found or is null.");
}

string? connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    if (builder.Environment.IsDevelopment())
    {
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is not defined in the configuration.");
    }
    else
    {
        // Fallback to Secret Manager in Production if env var not provided via Cloud Run --set-secrets
        connectionString = LoadSecret("db-connection-string");
    }
}

var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER")
    ?? (builder.Environment.IsDevelopment() ? "Sqlite" : "Postgres");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase))
    {
        var sqlitePath = builder.Configuration.GetValue<string>("Sqlite:DataSource") ?? "./data/app.db";
        options.UseSqlite($"Data Source={sqlitePath}");
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITranslationService, OpenAiTranslationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader()
        );
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    };

    c.AddSecurityRequirement(securityRequirement);
});

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://securetoken.google.com/flowershop-morales";
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://securetoken.google.com/flowershop-morales",
            ValidateAudience = true,
            ValidAudience = "flowershop-morales",
            ValidateLifetime = true
        };
    });

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwagger(c => { });
    app.UseSwaggerUI(c =>
    {
        foreach (var desc in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"FlowerShop API {desc.GroupName.ToUpperInvariant()}");
        }
    });
}

// HTTPS redirection disabled - Cloud Run handles HTTPS termination
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

internal sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "FlowerShop API",
                Version = description.ApiVersion.ToString()
            });
        }
    }
}