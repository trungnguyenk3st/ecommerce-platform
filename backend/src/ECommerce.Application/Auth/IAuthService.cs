namespace ECommerce.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
    Task<AuthResponse> RefreshAsync(string refreshToken, string ipAddress);
    Task RevokeRefreshTokenAsync(string refreshToken);
    Task<UserProfileResponse> GetProfileAsync(string userId);
    Task<UserProfileResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(string userId, ChangePasswordRequest request);
}
