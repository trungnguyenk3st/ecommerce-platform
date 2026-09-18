using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AppIdentityResult> RegisterAsync(string email, string password, string fullName)
    {
        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName };
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return new AppIdentityResult(false, null, result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Customer);
        return new AppIdentityResult(true, user.Id, Array.Empty<string>());
    }

    public async Task<AppIdentityResult> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return new AppIdentityResult(false, null, new[] { "Invalid email or password." });

        if (await _userManager.IsLockedOutAsync(user))
            return new AppIdentityResult(false, null, new[] { "This account has been locked." });

        var passwordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!passwordValid)
            return new AppIdentityResult(false, null, new[] { "Invalid email or password." });

        return new AppIdentityResult(true, user.Id, Array.Empty<string>());
    }

    public async Task<UserProfileDto?> GetUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var isLocked = await _userManager.IsLockedOutAsync(user);
        return new UserProfileDto(user.Id, user.Email!, user.FullName, roles, isLocked);
    }

    public async Task<IReadOnlyList<UserProfileDto>> GetAllCustomersAsync()
    {
        var customers = await _userManager.GetUsersInRoleAsync(Roles.Customer);
        var result = new List<UserProfileDto>();

        foreach (var user in customers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var isLocked = await _userManager.IsLockedOutAsync(user);
            result.Add(new UserProfileDto(user.Id, user.Email!, user.FullName, roles, isLocked));
        }

        return result.OrderByDescending(c => c.Id).ToList();
    }

    public async Task<bool> SetLockoutAsync(string userId, bool locked)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, locked ? DateTimeOffset.MaxValue : null);
        return true;
    }

    public async Task<AppIdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return new AppIdentityResult(false, null, new[] { "User not found." });

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return new AppIdentityResult(result.Succeeded, user.Id, result.Errors.Select(e => e.Description));
    }

    public async Task<AppIdentityResult> UpdateProfileAsync(string userId, string fullName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return new AppIdentityResult(false, null, new[] { "User not found." });

        user.FullName = fullName;
        var result = await _userManager.UpdateAsync(user);
        return new AppIdentityResult(result.Succeeded, user.Id, result.Errors.Select(e => e.Description));
    }
}
