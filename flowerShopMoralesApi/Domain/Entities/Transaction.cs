using System;

namespace flowerShopMoralesApi.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalSalePrice { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Operation { get; set; } = "sale";
    public string SenderName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<Sale> Sales { get; set; } = new();
}
