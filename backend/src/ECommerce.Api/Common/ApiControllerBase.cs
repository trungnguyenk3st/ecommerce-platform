using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Request reached an [Authorize] action without a NameIdentifier claim.");

    protected string CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

    protected string CurrentUserDisplayNameFallback => CurrentUserEmail.Split('@')[0];

    protected string ClientIpAddress
    {
        get
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
                return forwardedFor.Split(',')[0].Trim();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        }
    }
}
