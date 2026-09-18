using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Common.Interfaces;

/// <summary>
/// Persistence seam: the Application layer depends only on this, never on EF Core's DbContext
/// or the SQL Server provider directly. Implemented by ECommerce.Infrastructure.Persistence.AppDbContext.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<Address> Addresses { get; }
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
