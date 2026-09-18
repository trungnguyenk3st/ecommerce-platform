using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.OrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(o => o.UserId).HasMaxLength(450).IsRequired();
        builder.Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Property(o => o.DiscountAmount).HasPrecision(18, 2);
        builder.Property(o => o.ShippingFee).HasPrecision(18, 2);
        builder.Property(o => o.Total).HasPrecision(18, 2);
        builder.Property(o => o.CouponCode).HasMaxLength(50);
        builder.Property(o => o.ShippingFullName).HasMaxLength(200).IsRequired();
        builder.Property(o => o.ShippingPhone).HasMaxLength(20).IsRequired();
        builder.Property(o => o.ShippingLine1).HasMaxLength(300).IsRequired();
        builder.Property(o => o.ShippingLine2).HasMaxLength(300);
        builder.Property(o => o.ShippingCity).HasMaxLength(120).IsRequired();
        builder.Property(o => o.ShippingProvince).HasMaxLength(120).IsRequired();
        builder.Property(o => o.ShippingPostalCode).HasMaxLength(20);
        builder.Property(o => o.CustomerNote).HasMaxLength(2000);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.UserId);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.ProductImageUrl).HasMaxLength(1000);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Ignore(i => i.LineTotal);

        builder.HasOne(i => i.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.ProviderTransactionId).HasMaxLength(200);
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.RawResponse).HasMaxLength(4000);

        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
