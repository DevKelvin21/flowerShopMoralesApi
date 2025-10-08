using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using flowerShopMoralesApi.Api.DTOs;
using flowerShopMoralesApi.Infrastructure.Persistence;
using Asp.Versioning;

namespace flowerShopMoralesApi.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public HealthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetHealth()
    {
        var response = new HealthResponse
        {
            Timestamp = DateTime.UtcNow,
            Version = _configuration["ApiInfo:Version"] ?? "1.0.0"
        };

        // Check database connectivity
        try
        {
            // Attempt a simple query to check database connectivity
            await _context.Database.OpenConnectionAsync();
            await _context.Database.CloseConnectionAsync();
            
            // Determine database provider using same logic as Program.cs
            var dbProvider = Environment.GetEnvironmentVariable("DB_PROVIDER") ?? 
                           (HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() ? "Sqlite" : "Postgres");

            response.Database = new DatabaseHealth
            {
                IsConnected = true,
                Provider = dbProvider
            };
            
            response.Status = "Healthy";
        }
        catch (Exception)
        {
            response.Database = new DatabaseHealth
            {
                IsConnected = false,
                Provider = "Unknown"
            };
            
            response.Status = "Degraded";
        }

        return Ok(response);
    }
}
