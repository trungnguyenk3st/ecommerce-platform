using ECommerce.Application.Common;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Models;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Products;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<ProductCreateUpdateRequest> _validator;
    private readonly IValidator<ProductQueryParams> _queryValidator;
    private readonly IValidator<AdjustStockRequest> _stockValidator;

    public ProductService(
        IApplicationDbContext db,
        IValidator<ProductCreateUpdateRequest> validator,
        IValidator<ProductQueryParams> queryValidator,
        IValidator<AdjustStockRequest> stockValidator)
    {
        _db = db;
        _validator = validator;
        _queryValidator = queryValidator;
        _stockValidator = stockValidator;
    }

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(ProductQueryParams query)
    {
        await _queryValidator.ValidateAndThrowAsync(query);

        var products = _db.Products.Include(p => p.Category).Include(p => p.Images).AsNoTracking().AsQueryable();

        if (!query.IncludeInactive) products = products.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p => EF.Functions.Like(p.Name, $"%{term}%") || EF.Functions.Like(p.Sku, $"%{term}%"));
        }
        if (query.CategoryId.HasValue) products = products.Where(p => p.CategoryId == query.CategoryId.Value);
        if (query.MinPrice.HasValue) products = products.Where(p => p.Price >= query.MinPrice.Value);
        if (query.MaxPrice.HasValue) products = products.Where(p => p.Price <= query.MaxPrice.Value);
        if (query.InStockOnly == true) products = products.Where(p => p.StockQuantity > 0);

        products = query.SortBy switch
        {
            "price_asc" => products.OrderBy(p => p.Price),
            "price_desc" => products.OrderByDescending(p => p.Price),
            "rating" => products.OrderByDescending(p => p.AverageRating),
            "name" => products.OrderBy(p => p.Name),
            _ => products.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await products.CountAsync();
        var items = await products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => ToListItemDto(p))
            .ToListAsync();

        return PagedResult<ProductListItemDto>.Create(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<ProductDetailDto> GetByIdAsync(int id)
    {
        var product = await _db.Products.Include(p => p.Category).Include(p => p.Images).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(nameof(Product), id);
        return ToDetailDto(product);
    }

    public async Task<ProductDetailDto> GetBySlugAsync(string slug)
    {
        var product = await _db.Products.Include(p => p.Category).Include(p => p.Images).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug)
            ?? throw new NotFoundException(nameof(Product), slug);
        return ToDetailDto(product);
    }

    public async Task<ProductDetailDto> CreateAsync(ProductCreateUpdateRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);
        await EnsureCategoryExistsAsync(request.CategoryId);
        await EnsureSkuUniqueAsync(request.Sku, null);

        var product = new Product
        {
            Name = request.Name,
            Slug = await GenerateUniqueSlugAsync(request.Name, null),
            Description = request.Description,
            Sku = request.Sku,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            CategoryId = request.CategoryId,
            Images = request.Images.Select(i => new ProductImage { Url = i.Url, DisplayOrder = i.DisplayOrder, IsPrimary = i.IsPrimary }).ToList()
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(product.Id);
    }

    public async Task<ProductDetailDto> UpdateAsync(int id, ProductCreateUpdateRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);
        await EnsureCategoryExistsAsync(request.CategoryId);
        await EnsureSkuUniqueAsync(request.Sku, id);

        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(nameof(Product), id);

        if (!string.Equals(product.Name, request.Name, StringComparison.Ordinal))
            product.Slug = await GenerateUniqueSlugAsync(request.Name, id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Sku = request.Sku;
        product.Price = request.Price;
        product.CompareAtPrice = request.CompareAtPrice;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.CategoryId = request.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        _db.ProductImages.RemoveRange(product.Images);
        product.Images = request.Images.Select(i => new ProductImage { Url = i.Url, DisplayOrder = i.DisplayOrder, IsPrimary = i.IsPrimary }).ToList();

        await _db.SaveChangesAsync();
        return await GetByIdAsync(product.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(nameof(Product), id);

        var hasOrders = await _db.OrderItems.AnyAsync(oi => oi.ProductId == id);
        if (hasOrders)
        {
            // Preserve order history integrity: soft-delete instead of a hard delete.
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _db.Products.Remove(product);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<ProductDetailDto> AdjustStockAsync(int id, AdjustStockRequest request)
    {
        await _stockValidator.ValidateAndThrowAsync(request);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException(nameof(Product), id);

        if (request.DeltaQuantity > 0) product.IncreaseStock(request.DeltaQuantity);
        else product.DecreaseStock(-request.DeltaQuantity);

        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<List<ProductListItemDto>> GetRelatedAsync(int productId, int take = 8)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId)
            ?? throw new NotFoundException(nameof(Product), productId);

        var related = await _db.Products.Include(p => p.Category).Include(p => p.Images).AsNoTracking()
            .Where(p => p.CategoryId == product.CategoryId && p.Id != productId && p.IsActive)
            .OrderByDescending(p => p.AverageRating)
            .Take(take)
            .Select(p => ToListItemDto(p))
            .ToListAsync();

        return related;
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        var exists = await _db.Categories.AnyAsync(c => c.Id == categoryId);
        if (!exists) throw new NotFoundException(nameof(Category), categoryId);
    }

    private async Task EnsureSkuUniqueAsync(string sku, int? excludingId)
    {
        var duplicate = await _db.Products.AnyAsync(p => p.Sku == sku && p.Id != excludingId);
        if (duplicate) throw new DomainException($"SKU '{sku}' is already in use by another product.");
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludingId)
    {
        var baseSlug = Slugifier.Slugify(name);
        var slug = baseSlug;
        var suffix = 1;

        while (await _db.Products.AnyAsync(p => p.Slug == slug && p.Id != excludingId))
            slug = $"{baseSlug}-{++suffix}";

        return slug;
    }

    private static ProductListItemDto ToListItemDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.Price, p.CompareAtPrice,
        p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault()?.Url,
        p.StockQuantity > 0, p.AverageRating, p.ReviewCount, p.CategoryId, p.Category.Name);

    private static ProductDetailDto ToDetailDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.Description, p.Sku, p.Price, p.CompareAtPrice, p.StockQuantity, p.IsActive,
        p.AverageRating, p.ReviewCount, p.CategoryId, p.Category.Name,
        p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder)
            .Select(i => new ProductImageDto(i.Id, i.Url, i.DisplayOrder, i.IsPrimary)).ToList(),
        p.CreatedAt);
}
