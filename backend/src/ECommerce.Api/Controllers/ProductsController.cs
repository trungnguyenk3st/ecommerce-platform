using ECommerce.Api.Common;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Models;
using ECommerce.Application.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

public class ProductsController : ApiControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> Search([FromQuery] ProductQueryParams query)
        => Ok(await _productService.SearchAsync(query));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDetailDto>> GetById(int id) => Ok(await _productService.GetByIdAsync(id));

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDetailDto>> GetBySlug(string slug) => Ok(await _productService.GetBySlugAsync(slug));

    [HttpGet("{id:int}/related")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ProductListItemDto>>> GetRelated(int id) => Ok(await _productService.GetRelatedAsync(id));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductDetailDto>> Create(ProductCreateUpdateRequest request)
    {
        var created = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductDetailDto>> Update(int id, ProductCreateUpdateRequest request)
        => Ok(await _productService.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}/stock")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ProductDetailDto>> AdjustStock(int id, AdjustStockRequest request)
        => Ok(await _productService.AdjustStockAsync(id, request));
}
