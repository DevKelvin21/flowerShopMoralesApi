using System;

namespace flowerShopMoralesApi.Api.DTOs;

public class CreateSaleTransactionRequest
{
    public DateTime Date { get; set; }
    public decimal TotalSalePrice { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Operation { get; set; } = "sale";
    public string SenderName { get; set; } = string.Empty;
    public List<SaleItemDto> Sales { get; set; } = new();
}

public class SaleItemDto
{
    public string Item { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Quality { get; set; } = string.Empty;
}