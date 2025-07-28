using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using flowerShopMoralesApi.Domain.Entities;

namespace flowerShopMoralesApi.Infrastructure.Configurations;

public class LogConfiguration : IEntityTypeConfiguration<Log>
{
    public void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable("logs");
        builder.HasKey(l => l.Id);
    }
}
