namespace ECommerce.Application.Categories;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetTreeAsync(bool includeInactive = false);
    Task<List<CategoryDto>> GetFlatAsync(bool includeInactive = false);
    Task<CategoryDto> GetByIdAsync(int id);
    Task<CategoryDto> GetBySlugAsync(string slug);
    Task<CategoryDto> CreateAsync(CategoryCreateUpdateRequest request);
    Task<CategoryDto> UpdateAsync(int id, CategoryCreateUpdateRequest request);
    Task DeleteAsync(int id);
}
