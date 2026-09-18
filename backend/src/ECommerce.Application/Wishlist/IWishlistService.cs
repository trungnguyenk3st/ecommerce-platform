namespace ECommerce.Application.Wishlist;

public interface IWishlistService
{
    Task<List<WishlistItemDto>> GetAllAsync(string userId);
    Task<List<WishlistItemDto>> AddAsync(string userId, int productId);
    Task<List<WishlistItemDto>> RemoveAsync(string userId, int productId);
}
