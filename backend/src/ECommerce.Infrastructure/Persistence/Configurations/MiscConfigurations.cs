using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.Property(a => a.UserId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.FullName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Line1).HasMaxLength(300).IsRequired();
        builder.Property(a => a.Line2).HasMaxLength(300);
        builder.Property(a => a.City).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Province).HasMaxLength(120).IsRequired();
        builder.Property(a => a.PostalCode).HasMaxLength(20);
        builder.Property(a => a.Country).HasMaxLength(80).IsRequired();

        builder.HasIndex(a => a.UserId);
    }
}

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.Property(w => w.UserId).HasMaxLength(450).IsRequired();

        builder.HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.UserId).HasMaxLength(450).IsRequired();
        builder.Property(r => r.UserDisplayName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.ProductId, r.UserId }).IsUnique();
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.DiscountValue).HasPrecision(18, 2);
        builder.Property(c => c.MinOrderAmount).HasPrecision(18, 2);
        builder.Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);

        builder.HasIndex(c => c.Code).IsUnique();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(t => t.UserId).HasMaxLength(450).IsRequired();
        builder.Property(t => t.Token).HasMaxLength(200).IsRequired();
        builder.Property(t => t.ReplacedByToken).HasMaxLength(200);
        builder.Property(t => t.CreatedByIp).HasMaxLength(64);
        builder.Ignore(t => t.IsActive);

        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.UserId);
    }
}
