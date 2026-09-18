using ECommerce.Api.Common;
using ECommerce.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

public class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        => Ok(await _authService.RegisterAsync(request));

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        => Ok(await _authService.LoginAsync(request, ClientIpAddress));

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request)
        => Ok(await _authService.RefreshAsync(request.RefreshToken, ClientIpAddress));

    [HttpPost("revoke")]
    [AllowAnonymous]
    public async Task<IActionResult> Revoke(RefreshTokenRequest request)
    {
        await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> Me()
        => Ok(await _authService.GetProfileAsync(CurrentUserId));

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> UpdateMe(UpdateProfileRequest request)
        => Ok(await _authService.UpdateProfileAsync(CurrentUserId, request));

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        await _authService.ChangePasswordAsync(CurrentUserId, request);
        return NoContent();
    }
}
