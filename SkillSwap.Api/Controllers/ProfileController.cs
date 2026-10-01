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

        var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/pjpeg", "image/x-png" };
        if (!allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            return BadRequest(ApiResponse<object>.Failure("Invalid image content type."));

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<object>.Failure("File size exceeds 5MB limit."));

        // Validate image magic bytes
        byte[] header = new byte[12];
        using (var reader = file.OpenReadStream())
        {
            var bytesRead = await reader.ReadAsync(header, 0, header.Length);
            if (bytesRead < 4)
                return BadRequest(ApiResponse<object>.Failure("Invalid file content."));

            bool isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
            bool isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
            bool isWebp = bytesRead >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
                          header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

            if (!isJpeg && !isPng && !isWebp)
                return BadRequest(ApiResponse<object>.Failure("File header signature does not match a valid image format."));
        }

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
