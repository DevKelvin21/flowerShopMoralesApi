using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using flowerShopMoralesApi.Domain.Entities;

namespace flowerShopMoralesApi.Infrastructure.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");
        builder.HasKey(t => t.Id);

        builder.HasMany(t => t.Sales)
               .WithOne()
               .HasForeignKey(s => s.TransactionId);
    }
}
