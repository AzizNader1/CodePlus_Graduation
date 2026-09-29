using Microsoft.AspNetCore.Identity;

namespace SkillSwap.Domain.Entities;

/// <summary>
/// Domain role entity for role-based access control.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base()
    {
        Id = Guid.NewGuid();
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
        Id = Guid.NewGuid();
    }
}
