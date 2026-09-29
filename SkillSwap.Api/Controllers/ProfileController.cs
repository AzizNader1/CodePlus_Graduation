using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Application.Features.Profile;

namespace SkillSwap.Api.Controllers;

[Authorize]
public class ProfileController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ProfileController(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult> GetProfile()
    {
        var result = await Mediator.Send(new GetProfileQuery());
        return HandleResult(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var result = await Mediator.Send(new UpdateProfileCommand(request));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Failure("File not provided."));

        if (_currentUser.UserId == null)
            return Unauthorized(ApiResponse<object>.Failure("Unauthorized.", statusCode: StatusCodes.Status401Unauthorized));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest(ApiResponse<object>.Failure("Only image files (.jpg, .jpeg, .png, .webp) are allowed."));

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<object>.Failure("File size exceeds 5MB limit."));

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{_currentUser.UserId.Value}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var avatarUrl = $"/avatars/{fileName}";
        var user = await _context.Users.FindAsync(new object[] { _currentUser.UserId.Value });
        if (user != null)
        {
            user.AvatarUrl = avatarUrl;
            await _context.SaveChangesAsync();
        }

        return Ok(ApiResponse<object>.Success(new { avatarUrl }, "Avatar uploaded successfully."));
    }

    [HttpPut]
    public async Task<ActionResult> SetAvailability([FromBody] ICollection<UserAvailabilityDTO> slots)
    {
        var result = await Mediator.Send(new SetAvailabilityCommand(slots));
        return HandleResult(result);
    }

    [HttpGet("{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetPublicUserProfile(Guid userId)
    {
        var result = await Mediator.Send(new GetPublicUserProfileQuery(userId));
        return HandleResult(result);
    }
}
