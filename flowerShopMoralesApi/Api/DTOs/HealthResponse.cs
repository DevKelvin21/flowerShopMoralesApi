namespace flowerShopMoralesApi.Api.DTOs;

public class HealthResponse
{
    public string Status { get; set; } = string.Empty;  // "Healthy" or "Degraded"
    public DateTime Timestamp { get; set; }
    public DatabaseHealth Database { get; set; } = new();
    public string Version { get; set; } = string.Empty;
}

public class DatabaseHealth
{
    public bool IsConnected { get; set; }
    public string Provider { get; set; } = string.Empty;  // "Sqlite" or "Postgres"
}
