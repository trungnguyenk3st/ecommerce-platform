using ECommerce.Api.Common;
using ECommerce.Application.Categories;
using ECommerce.Application.Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

public class CategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetTree() => Ok(await _categoryService.GetTreeAsync());

    [HttpGet("flat")]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryDto>>> GetFlat() => Ok(await _categoryService.GetFlatAsync());

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetById(int id) => Ok(await _categoryService.GetByIdAsync(id));

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryDto>> GetBySlug(string slug) => Ok(await _categoryService.GetBySlugAsync(slug));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CategoryDto>> Create(CategoryCreateUpdateRequest request)
    {
        var created = await _categoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryCreateUpdateRequest request)
        => Ok(await _categoryService.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
