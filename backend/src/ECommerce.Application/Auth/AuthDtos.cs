namespace ECommerce.Application.Auth;

public record RegisterRequest(string Email, string Password, string FullName);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    string UserId,
    string Email,
    string FullName,
    IList<string> Roles);

public record UserProfileResponse(string Id, string Email, string FullName, IList<string> Roles);
public record UpdateProfileRequest(string FullName);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
