using SkillSwap.Application.Common.Models;
using SkillSwap.Domain.Entities;

namespace SkillSwap.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(Result Result, Guid UserId)> CreateUserAsync(string email, string password, string fullName, string? location, string? timeZone);
    Task<Result> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<Result> AddToRoleAsync(ApplicationUser user, string role);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<Result> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
}
