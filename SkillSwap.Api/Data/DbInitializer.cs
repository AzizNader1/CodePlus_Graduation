using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;
using SkillSwap.Infrastructure.Persistence;

namespace SkillSwap.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // 1. Apply any pending database migrations
        await context.Database.MigrateAsync();

        // 2. Seed Roles
        string[] roles = { "Admin", "User", "Moderator" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        // 3. Seed Default Admin User
        var adminEmail = "admin@skillswap.app";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Platform Administrator",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // 4. Seed Standard Categories & Skills
        if (!await context.Categories.AnyAsync())
        {
            var devCategory = new Category
            {
                Name = "Software Development",
                Description = "Web, Mobile, Desktop, and Cloud programming technologies.",
                IconUrl = "code",
                DisplayOrder = 1
            };

            var langCategory = new Category
            {
                Name = "Languages & Culture",
                Description = "Foreign language speaking, listening, and cultural exchange.",
                IconUrl = "globe",
                DisplayOrder = 2
            };

            var designCategory = new Category
            {
                Name = "Design & Multimedia",
                Description = "UI/UX, Graphic Design, 3D modeling, and visual arts.",
                IconUrl = "palette",
                DisplayOrder = 3
            };

            var musicCategory = new Category
            {
                Name = "Music & Arts",
                Description = "Musical instruments, vocal training, and audio production.",
                IconUrl = "music",
                DisplayOrder = 4
            };

            context.Categories.AddRange(devCategory, langCategory, designCategory, musicCategory);
            await context.SaveChangesAsync();

            // Seed Skills
            var csharp = new Skill { CategoryId = devCategory.Id, Name = "C# & .NET", Description = "Enterprise backend, Web APIs, and Clean Architecture." };
            var python = new Skill { CategoryId = devCategory.Id, Name = "Python", Description = "Data science, scripting, and backend automation." };
            var react = new Skill { CategoryId = devCategory.Id, Name = "React & Frontend", Description = "Modern component-based responsive web applications." };

            var spanish = new Skill { CategoryId = langCategory.Id, Name = "Spanish", Description = "Conversational Spanish grammar and pronunciation." };
            var english = new Skill { CategoryId = langCategory.Id, Name = "English", Description = "Business and conversational English fluency." };

            var figma = new Skill { CategoryId = designCategory.Id, Name = "Figma & UI/UX", Description = "Wireframing, prototyping, and design systems." };
            var guitar = new Skill { CategoryId = musicCategory.Id, Name = "Acoustic Guitar", Description = "Chords, fingerstyle, and rhythm mastery." };

            context.Skills.AddRange(csharp, python, react, spanish, english, figma, guitar);
            await context.SaveChangesAsync();

            // 5. Seed Test Users (Alice & Bob for instant reciprocal match)
            var aliceEmail = "alice@skillswap.app";
            var alice = await userManager.FindByEmailAsync(aliceEmail);
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = aliceEmail,
                    Email = aliceEmail,
                    FullName = "Alice Johnson",
                    Bio = "Senior Full Stack .NET Developer eager to learn conversational Spanish.",
                    Location = "Cairo, Egypt",
                    TimeZone = "UTC+2",
                    AverageRating = 4.9,
                    TotalReviewsCount = 12,
                    TotalSwapsCompleted = 8,
                    EmailConfirmed = true,
                    IsActive = true
                };
                await userManager.CreateAsync(alice, "Password@123");
                await userManager.AddToRoleAsync(alice, "User");

                // Alice teaches C# and wants Spanish
                context.UserSkills.Add(new UserSkill
                {
                    UserId = alice.Id,
                    SkillId = csharp.Id,
                    ProficiencyLevel = ProficiencyLevel.Expert,
                    YearsOfExperience = 5,
                    Description = "5+ years developing enterprise ASP.NET Core Clean Architecture solutions."
                });

                context.UserDesiredSkills.Add(new UserDesiredSkill
                {
                    UserId = alice.Id,
                    SkillId = spanish.Id,
                    TargetLevel = ProficiencyLevel.Intermediate,
                    Priority = SkillPriority.High
                });
            }

            var bobEmail = "bob@skillswap.app";
            var bob = await userManager.FindByEmailAsync(bobEmail);
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = bobEmail,
                    Email = bobEmail,
                    FullName = "Bob Martinez",
                    Bio = "Native Spanish instructor passionate about learning backend C# programming.",
                    Location = "Madrid, Spain",
                    TimeZone = "UTC+1",
                    AverageRating = 4.8,
                    TotalReviewsCount = 9,
                    TotalSwapsCompleted = 6,
                    EmailConfirmed = true,
                    IsActive = true
                };
                await userManager.CreateAsync(bob, "Password@123");
                await userManager.AddToRoleAsync(bob, "User");

                // Bob teaches Spanish and wants C#
                context.UserSkills.Add(new UserSkill
                {
                    UserId = bob.Id,
                    SkillId = spanish.Id,
                    ProficiencyLevel = ProficiencyLevel.Expert,
                    YearsOfExperience = 7,
                    Description = "Native speaker and certified language instructor."
                });

                context.UserDesiredSkills.Add(new UserDesiredSkill
                {
                    UserId = bob.Id,
                    SkillId = csharp.Id,
                    TargetLevel = ProficiencyLevel.Beginner,
                    Priority = SkillPriority.High
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
