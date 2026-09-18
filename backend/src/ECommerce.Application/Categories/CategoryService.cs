using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Categories;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CategoryCreateUpdateRequest> _validator;

    public CategoryService(IApplicationDbContext db, IValidator<CategoryCreateUpdateRequest> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<List<CategoryDto>> GetTreeAsync(bool includeInactive = false)
    {
        var all = await GetFlatAsync(includeInactive);
        var byId = all.ToDictionary(c => c.Id);
        var roots = new List<CategoryDto>();

        foreach (var category in all)
        {
            if (category.ParentCategoryId is int parentId && byId.TryGetValue(parentId, out var parent))
                parent.SubCategories.Add(category);
            else
                roots.Add(category);
        }

        return roots;
    }

    public async Task<List<CategoryDto>> GetFlatAsync(bool includeInactive = false)
    {
        var query = _db.Categories.Include(c => c.Products).AsNoTracking().AsQueryable();
        if (!includeInactive) query = query.Where(c => c.IsActive);

        var categories = await query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToListAsync();
        return categories.Select(ToDto).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _db.Categories.Include(c => c.Products).AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException(nameof(Category), id);
        return ToDto(category);
    }

    public async Task<CategoryDto> GetBySlugAsync(string slug)
    {
        var category = await _db.Categories.Include(c => c.Products).AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug)
            ?? throw new NotFoundException(nameof(Category), slug);
        return ToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CategoryCreateUpdateRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var category = new Category
        {
            Name = request.Name,
            Slug = await GenerateUniqueSlugAsync(request.Name, null),
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            ParentCategoryId = request.ParentCategoryId
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return await GetByIdAsync(category.Id);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CategoryCreateUpdateRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException(nameof(Category), id);

        if (request.ParentCategoryId == id)
            throw new DomainException("A category cannot be its own parent.");

        if (!string.Equals(category.Name, request.Name, StringComparison.Ordinal))
            category.Slug = await GenerateUniqueSlugAsync(request.Name, id);

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.IsActive = request.IsActive;
        category.DisplayOrder = request.DisplayOrder;
        category.ParentCategoryId = request.ParentCategoryId;
        category.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(category.Id);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _db.Categories.Include(c => c.Products).Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException(nameof(Category), id);

        if (category.Products.Count > 0)
            throw new DomainException("Cannot delete a category that still has products. Move or delete its products first.");
        if (category.SubCategories.Count > 0)
            throw new DomainException("Cannot delete a category that has subcategories.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludingId)
    {
        var baseSlug = Slugifier.Slugify(name);
        var slug = baseSlug;
        var suffix = 1;

        while (await _db.Categories.AnyAsync(c => c.Slug == slug && c.Id != excludingId))
            slug = $"{baseSlug}-{++suffix}";

        return slug;
    }

    private static CategoryDto ToDto(Category c) => new(
        c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.IsActive, c.DisplayOrder,
        c.ParentCategoryId, c.Products.Count, new List<CategoryDto>());
}
