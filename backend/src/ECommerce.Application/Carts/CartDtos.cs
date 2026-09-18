namespace ECommerce.Application.Carts;

public record CartItemDto(
    int Id,
    int ProductId,
    string ProductName,
    string? ProductImageUrl,
    string ProductSlug,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    int AvailableStock);

public record CartDto(int Id, List<CartItemDto> Items, decimal Subtotal, int TotalItems);

public record AddCartItemRequest(int ProductId, int Quantity);
public record UpdateCartItemRequest(int Quantity);
