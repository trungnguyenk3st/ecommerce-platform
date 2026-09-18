namespace ECommerce.Application.Categories;

public record CategoryDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    int DisplayOrder,
    int? ParentCategoryId,
    int ProductCount,
    List<CategoryDto> SubCategories);

public record CategoryCreateUpdateRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    bool IsActive,
    int DisplayOrder,
    int? ParentCategoryId);
