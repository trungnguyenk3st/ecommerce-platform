using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Carts;

public class CartService : ICartService
{
    private readonly IApplicationDbContext _db;

    public CartService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart = await GetOrCreateCartEntityAsync(userId);
        return ToDto(cart);
    }

    public async Task<CartDto> AddItemAsync(string userId, AddCartItemRequest request)
    {
        if (request.Quantity <= 0) throw new DomainException("Quantity must be positive.");

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var cart = await GetOrCreateCartEntityAsync(userId);
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        var desiredQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (desiredQuantity > product.StockQuantity)
            throw new DomainException($"Only {product.StockQuantity} unit(s) of '{product.Name}' are available.");

        if (existingItem is not null)
            existingItem.Quantity = desiredQuantity;
        else
            _db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = request.ProductId, Quantity = request.Quantity });

        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> UpdateItemAsync(string userId, int cartItemId, UpdateCartItemRequest request)
    {
        if (request.Quantity <= 0) throw new DomainException("Quantity must be positive.");

        var item = await _db.CartItems.Include(i => i.Cart).Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId)
            ?? throw new NotFoundException("CartItem", cartItemId);

        if (request.Quantity > item.Product.StockQuantity)
            throw new DomainException($"Only {item.Product.StockQuantity} unit(s) of '{item.Product.Name}' are available.");

        item.Quantity = request.Quantity;
        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> RemoveItemAsync(string userId, int cartItemId)
    {
        var item = await _db.CartItems.Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId)
            ?? throw new NotFoundException("CartItem", cartItemId);

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    public async Task<CartDto> ClearAsync(string userId)
    {
        var cart = await GetOrCreateCartEntityAsync(userId);
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();
        return await GetCartAsync(userId);
    }

    private async Task<Domain.Entities.Cart> GetOrCreateCartEntityAsync(string userId)
    {
        var cart = await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is not null) return cart;

        cart = new Domain.Entities.Cart { UserId = userId };
        _db.Carts.Add(cart);
        await _db.SaveChangesAsync();
        return cart;
    }

    private static CartDto ToDto(Domain.Entities.Cart cart)
    {
        var items = cart.Items
            .OrderBy(i => i.Id)
            .Select(i => new CartItemDto(
                i.Id, i.ProductId, i.Product.Name,
                i.Product.Images.OrderByDescending(img => img.IsPrimary).ThenBy(img => img.DisplayOrder).FirstOrDefault()?.Url,
                i.Product.Slug, i.Product.Price, i.Quantity, i.Product.Price * i.Quantity, i.Product.StockQuantity))
            .ToList();

        return new CartDto(cart.Id, items, items.Sum(i => i.LineTotal), items.Sum(i => i.Quantity));
    }
}
