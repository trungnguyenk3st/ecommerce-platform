namespace ECommerce.Application.Carts;

public interface ICartService
{
    Task<CartDto> GetCartAsync(string userId);
    Task<CartDto> AddItemAsync(string userId, AddCartItemRequest request);
    Task<CartDto> UpdateItemAsync(string userId, int cartItemId, UpdateCartItemRequest request);
    Task<CartDto> RemoveItemAsync(string userId, int cartItemId);
    Task<CartDto> ClearAsync(string userId);
}
