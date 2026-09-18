using ECommerce.Api.Common;
using ECommerce.Application.Wishlist;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize]
public class WishlistController : ApiControllerBase
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    [HttpGet]
    public async Task<ActionResult<List<WishlistItemDto>>> GetAll() => Ok(await _wishlistService.GetAllAsync(CurrentUserId));

    [HttpPost("{productId:int}")]
    public async Task<ActionResult<List<WishlistItemDto>>> Add(int productId)
        => Ok(await _wishlistService.AddAsync(CurrentUserId, productId));

    [HttpDelete("{productId:int}")]
    public async Task<ActionResult<List<WishlistItemDto>>> Remove(int productId)
        => Ok(await _wishlistService.RemoveAsync(CurrentUserId, productId));
}
