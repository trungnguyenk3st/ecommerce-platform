namespace ECommerce.Application.Products;

public record ProductImageDto(int Id, string Url, int DisplayOrder, bool IsPrimary);

public record ProductListItemDto(
    int Id,
    string Name,
    string Slug,
    decimal Price,
    decimal? CompareAtPrice,
    string? PrimaryImageUrl,
    bool InStock,
    double AverageRating,
    int ReviewCount,
    int CategoryId,
    string CategoryName);

public record ProductDetailDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string Sku,
    decimal Price,
    decimal? CompareAtPrice,
    int StockQuantity,
    bool IsActive,
    double AverageRating,
    int ReviewCount,
    int CategoryId,
    string CategoryName,
    List<ProductImageDto> Images,
    DateTime CreatedAt);

public record ProductQueryParams
{
    public string? Search { get; init; }
    public int? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public bool? InStockOnly { get; init; }
    public string? SortBy { get; init; } // "price_asc" | "price_desc" | "newest" | "rating" | "name"
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public bool IncludeInactive { get; init; } = false;
}

public record ProductImageInput(string Url, int DisplayOrder, bool IsPrimary);

public record ProductCreateUpdateRequest(
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    decimal? CompareAtPrice,
    int StockQuantity,
    bool IsActive,
    int CategoryId,
    List<ProductImageInput> Images);

public record AdjustStockRequest(int DeltaQuantity, string? Reason);
