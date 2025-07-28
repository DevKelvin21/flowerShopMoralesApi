using System;
using flowerShopMoralesApi.Api.DTOs;

namespace flowerShopMoralesApi.Application.Interfaces;

public interface ITransactionService
{
    Task<Guid> CreateSaleTransactionAsync(CreateSaleTransactionRequest request);
}
