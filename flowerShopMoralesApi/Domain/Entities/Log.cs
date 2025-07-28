using System;

namespace flowerShopMoralesApi.Domain.Entities;

public class Log
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string MessageContent { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public Guid TransactionId { get; set; }
}
