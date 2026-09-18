using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Auth;

public class AuthService : IAuthService
{
    private const int RefreshTokenDays = 7;

    private readonly IApplicationDbContext _db;
    private readonly IIdentityService _identity;
    private readonly IJwtTokenService _jwt;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;

    public AuthService(
        IApplicationDbContext db,
        IIdentityService identity,
        IJwtTokenService jwt,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator)
    {
        _db = db;
        _identity = identity;
        _jwt = jwt;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _changePasswordValidator = changePasswordValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        await _registerValidator.ValidateAndThrowAsync(request);

        var result = await _identity.RegisterAsync(request.Email, request.Password, request.FullName);
        if (!result.Succeeded || result.UserId is null)
            throw new AppValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("email", string.Join(" ", result.Errors))
            });

        return await IssueTokensAsync(result.UserId, request.Email, request.FullName, new List<string> { Roles.Customer }, "0.0.0.0");
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
    {
        await _loginValidator.ValidateAndThrowAsync(request);

        var result = await _identity.ValidateCredentialsAsync(request.Email, request.Password);
        if (!result.Succeeded || result.UserId is null)
            throw new UnauthorizedException("Invalid email or password.");

        var profile = await _identity.GetUserAsync(result.UserId)
            ?? throw new UnauthorizedException("Invalid email or password.");

        return await IssueTokensAsync(profile.Id, profile.Email, profile.FullName, profile.Roles, ipAddress);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, string ipAddress)
    {
        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (existing is null || !existing.IsActive)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var profile = await _identity.GetUserAsync(existing.UserId)
            ?? throw new UnauthorizedException("Invalid or expired refresh token.");

        var response = await IssueTokensAsync(profile.Id, profile.Email, profile.FullName, profile.Roles, ipAddress);

        existing.RevokedAtUtc = DateTime.UtcNow;
        existing.ReplacedByToken = response.RefreshToken;
        await _db.SaveChangesAsync();

        return response;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);
        if (existing is null || !existing.IsActive) return;

        existing.RevokedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<UserProfileResponse> GetProfileAsync(string userId)
    {
        var profile = await _identity.GetUserAsync(userId) ?? throw new Domain.Exceptions.NotFoundException("User", userId);
        return new UserProfileResponse(profile.Id, profile.Email, profile.FullName, profile.Roles);
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var result = await _identity.UpdateProfileAsync(userId, request.FullName);
        if (!result.Succeeded)
            throw new AppValidationException(new[] { new FluentValidation.Results.ValidationFailure("fullName", string.Join(" ", result.Errors)) });

        return await GetProfileAsync(userId);
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        await _changePasswordValidator.ValidateAndThrowAsync(request);

        var result = await _identity.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            throw new AppValidationException(new[] { new FluentValidation.Results.ValidationFailure("currentPassword", string.Join(" ", result.Errors)) });
    }

    private async Task<AuthResponse> IssueTokensAsync(string userId, string email, string fullName, IList<string> roles, string ipAddress)
    {
        var accessToken = _jwt.GenerateAccessToken(userId, email, roles);
        var refreshTokenValue = _jwt.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = refreshTokenValue,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(RefreshTokenDays),
            CreatedByIp = ipAddress
        });
        await _db.SaveChangesAsync();

        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshTokenValue,
            userId,
            email,
            fullName,
            roles);
    }
}
