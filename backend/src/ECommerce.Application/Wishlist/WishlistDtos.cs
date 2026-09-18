namespace ECommerce.Application.Wishlist;

public record WishlistItemDto(int Id, int ProductId, string ProductName, string ProductSlug, string? ProductImageUrl, decimal Price, bool InStock);
