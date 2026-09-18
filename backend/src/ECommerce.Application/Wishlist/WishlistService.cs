using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Wishlist;

public class WishlistService : IWishlistService
{
    private readonly IApplicationDbContext _db;

    public WishlistService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<WishlistItemDto>> GetAllAsync(string userId)
    {
        var items = await _db.WishlistItems.Include(w => w.Product).ThenInclude(p => p.Images).AsNoTracking()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return items.Select(ToDto).ToList();
    }

    public async Task<List<WishlistItemDto>> AddAsync(string userId, int productId)
    {
        var productExists = await _db.Products.AnyAsync(p => p.Id == productId);
        if (!productExists) throw new NotFoundException(nameof(Product), productId);

        var alreadyAdded = await _db.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        if (!alreadyAdded)
        {
            _db.WishlistItems.Add(new WishlistItem { UserId = userId, ProductId = productId });
            await _db.SaveChangesAsync();
        }

        return await GetAllAsync(userId);
    }

    public async Task<List<WishlistItemDto>> RemoveAsync(string userId, int productId)
    {
        var item = await _db.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        if (item is not null)
        {
            _db.WishlistItems.Remove(item);
            await _db.SaveChangesAsync();
        }

        return await GetAllAsync(userId);
    }

    private static WishlistItemDto ToDto(WishlistItem w) => new(
        w.Id, w.ProductId, w.Product.Name, w.Product.Slug,
        w.Product.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.Url,
        w.Product.Price, w.Product.StockQuantity > 0);
}
