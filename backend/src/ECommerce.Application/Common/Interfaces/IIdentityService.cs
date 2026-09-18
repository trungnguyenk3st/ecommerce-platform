namespace ECommerce.Application.Common.Interfaces;

public record AppIdentityResult(bool Succeeded, string? UserId, IEnumerable<string> Errors);

public record UserProfileDto(string Id, string Email, string FullName, IList<string> Roles, bool IsLocked);

public interface IIdentityService
{
    Task<AppIdentityResult> RegisterAsync(string email, string password, string fullName);
    Task<AppIdentityResult> ValidateCredentialsAsync(string email, string password);
    Task<UserProfileDto?> GetUserAsync(string userId);
    Task<IReadOnlyList<UserProfileDto>> GetAllCustomersAsync();
    Task<bool> SetLockoutAsync(string userId, bool locked);
    Task<AppIdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    Task<AppIdentityResult> UpdateProfileAsync(string userId, string fullName);
}
