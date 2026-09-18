using ECommerce.Application.Common.Models;

namespace ECommerce.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductListItemDto>> SearchAsync(ProductQueryParams query);
    Task<ProductDetailDto> GetByIdAsync(int id);
    Task<ProductDetailDto> GetBySlugAsync(string slug);
    Task<ProductDetailDto> CreateAsync(ProductCreateUpdateRequest request);
    Task<ProductDetailDto> UpdateAsync(int id, ProductCreateUpdateRequest request);
    Task DeleteAsync(int id);
    Task<ProductDetailDto> AdjustStockAsync(int id, AdjustStockRequest request);
    Task<List<ProductListItemDto>> GetRelatedAsync(int productId, int take = 8);
}
