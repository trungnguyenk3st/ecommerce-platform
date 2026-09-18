namespace ECommerce.Application.Common.Interfaces;

public record AccessTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    AccessTokenResult GenerateAccessToken(string userId, string email, IList<string> roles);
    string GenerateRefreshToken();
}
