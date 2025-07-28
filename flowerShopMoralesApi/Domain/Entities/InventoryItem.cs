using System;

namespace flowerShopMoralesApi.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; set; }
    public string Item { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
