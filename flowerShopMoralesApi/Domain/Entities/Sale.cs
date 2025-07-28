using System;

namespace flowerShopMoralesApi.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string Item { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Quality { get; set; } = string.Empty;
}