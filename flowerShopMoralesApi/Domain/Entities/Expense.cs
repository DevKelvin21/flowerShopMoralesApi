using System;

namespace flowerShopMoralesApi.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
