using Microsoft.AspNetCore.Identity;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<(Result Result, Guid UserId)> CreateUserAsync(
        string email, string password, string fullName, string? location, string? timeZone)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            Location = location,
            TimeZone = timeZone,
            IsActive = true,
            EmailConfirmed = true // Default auto-confirmed for seamless flow
        };

        var identityResult = await _userManager.CreateAsync(user, password);
        if (!identityResult.Succeeded)
        {
            return (Result.Failure(identityResult.Errors.Select(e => e.Description)), Guid.Empty);
        }

        // Ensure "User" role exists
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new ApplicationRole("User"));
        }

        await _userManager.AddToRoleAsync(user, "User");
        return (Result.Success(), user.Id);
    }

    public async Task<Result> CheckPasswordAsync(ApplicationUser user, string password)
    {
        var isMatch = await _userManager.CheckPasswordAsync(user, password);
        return isMatch ? Result.Success() : Result.Failure("Invalid credentials.");
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<Result> AddToRoleAsync(ApplicationUser user, string role)
    {
        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new ApplicationRole(role));
        }

        var res = await _userManager.AddToRoleAsync(user, role);
        return res.Succeeded ? Result.Success() : Result.Failure(res.Errors.Select(e => e.Description));
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        var res = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return res.Succeeded ? Result.Success() : Result.Failure(res.Errors.Select(e => e.Description));
    }
}
