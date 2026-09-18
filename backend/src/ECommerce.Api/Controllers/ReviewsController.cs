using ECommerce.Api.Common;
using ECommerce.Application.Common.Constants;
using ECommerce.Application.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

public class ReviewsController : ApiControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("~/api/products/{productId:int}/reviews")]
    [AllowAnonymous]
    public async Task<ActionResult<List<ReviewDto>>> GetForProduct(int productId)
        => Ok(await _reviewService.GetForProductAsync(productId));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> Create(CreateReviewRequest request)
    {
        var displayName = string.IsNullOrWhiteSpace(CurrentUserEmail) ? "Customer" : CurrentUserDisplayNameFallback;
        return Ok(await _reviewService.CreateAsync(CurrentUserId, displayName, request));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        await _reviewService.DeleteAsync(id);
        return NoContent();
    }
}
