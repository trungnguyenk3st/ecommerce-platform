using ECommerce.Api.Common;
using ECommerce.Application.Carts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Authorize]
public class CartController : ApiControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get() => Ok(await _cartService.GetCartAsync(CurrentUserId));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemRequest request)
        => Ok(await _cartService.AddItemAsync(CurrentUserId, request));

    [HttpPut("items/{cartItemId:int}")]
    public async Task<ActionResult<CartDto>> UpdateItem(int cartItemId, UpdateCartItemRequest request)
        => Ok(await _cartService.UpdateItemAsync(CurrentUserId, cartItemId, request));

    [HttpDelete("items/{cartItemId:int}")]
    public async Task<ActionResult<CartDto>> RemoveItem(int cartItemId)
        => Ok(await _cartService.RemoveItemAsync(CurrentUserId, cartItemId));

    [HttpDelete]
    public async Task<ActionResult<CartDto>> Clear() => Ok(await _cartService.ClearAsync(CurrentUserId));
}
