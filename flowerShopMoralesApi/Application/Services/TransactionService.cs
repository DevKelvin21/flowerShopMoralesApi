using System;
using Microsoft.EntityFrameworkCore;
using flowerShopMoralesApi.Application.Interfaces;
using flowerShopMoralesApi.Domain.Entities;
using flowerShopMoralesApi.Api.DTOs;
using flowerShopMoralesApi.Infrastructure.Persistence;

namespace flowerShopMoralesApi.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _context;

    public TransactionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> CreateSaleTransactionAsync(CreateSaleTransactionRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            Date = request.Date,
            TotalSalePrice = request.TotalSalePrice,
            PaymentMethod = request.PaymentMethod,
            Operation = request.Operation,
            SenderName = request.SenderName,
            Sales = request.Sales.Select(s => new Sale
            {
                Id = Guid.NewGuid(),
                Item = s.Item,
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice,
                Quality = s.Quality
            }).ToList()
        };

        await _context.Transactions.AddAsync(tx);
        await _context.SaveChangesAsync();

        foreach (var sale in tx.Sales)
        {
            sale.TransactionId = tx.Id;
            _context.Sales.Add(sale);

            var inventoryItem = await _context.Inventory
                .FirstOrDefaultAsync(i => i.Item == sale.Item && i.Quality == sale.Quality);

            if (inventoryItem == null || inventoryItem.Quantity < sale.Quantity)
                throw new InvalidOperationException("Insufficient inventory");

            inventoryItem.Quantity -= sale.Quantity;
            inventoryItem.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        await _context.Logs.AddAsync(new Log
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            OperationType = "sale",
            MessageContent = $"Registered sale for {request.SenderName}",
            UserName = request.SenderName,
            TransactionId = tx.Id
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return tx.Id;
    }
}
