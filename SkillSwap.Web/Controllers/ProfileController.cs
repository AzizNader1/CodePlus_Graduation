using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public ProfileController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var response = await _apiClient.GetAsync<UserProfileDTO>("Profile/GetProfile");
        if (response == null || !response.IsSuccess || response.Data == null)
        {
            TempData["Error"] = response?.Message ?? "Unable to load profile.";
            return RedirectToAction("Index", "Home");
        }

        return View(response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var profileTask = _apiClient.GetAsync<UserProfileDTO>("Profile/GetProfile");
        var skillsTask = _apiClient.GetAsync<List<SkillDTO>>("Skills/GetSkills");

        await Task.WhenAll(profileTask, skillsTask);

        var profile = profileTask.Result?.Data;
        ViewBag.MasterSkills = skillsTask.Result?.Data ?? new List<SkillDTO>();

        if (profile == null)
        {
            TempData["Error"] = "Unable to load profile for editing.";
            return RedirectToAction("Index");
        }

        var updateReq = new UpdateProfileRequest
        {
            FullName = profile.FullName,
            Bio = profile.Bio,
            Location = profile.Location
        };

        ViewBag.Profile = profile;
        return View(updateReq);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var response = await _apiClient.PutCommandAsync("Profile/UpdateProfile", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to update profile.";
            return RedirectToAction("Edit");
        }

        TempData["Success"] = "Profile details updated successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select an image file to upload.";
            return RedirectToAction("Index");
        }

        var response = await _apiClient.PostMultipartAsync<object>("Profile/UploadAvatar", file);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to upload avatar.";
            return RedirectToAction("Index");
        }

        TempData["Success"] = "Avatar uploaded successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOfferedSkill(AddOfferedSkillRequest request)
    {
        var response = await _apiClient.PostAsync<UserSkillDTO>("Skills/AddOfferedSkill", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to add offered skill.";
        }
        else
        {
            TempData["Success"] = "Offered skill added to profile!";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOfferedSkill(Guid userSkillId)
    {
        var response = await _apiClient.DeleteCommandAsync($"Skills/DeleteOfferedSkill/{userSkillId}");

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to remove skill.";
        }
        else
        {
            TempData["Success"] = "Offered skill removed successfully.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWantedSkill(AddWantedSkillRequest request)
    {
        var response = await _apiClient.PostAsync<UserDesiredSkillDTO>("Skills/AddWantedSkill", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to add wanted skill.";
        }
        else
        {
            TempData["Success"] = "Desired skill added to your wishlist!";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWantedSkill(Guid wantedSkillId)
    {
        var response = await _apiClient.DeleteCommandAsync($"Skills/DeleteWantedSkill/{wantedSkillId}");

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to remove wanted skill.";
        }
        else
        {
            TempData["Success"] = "Skill removed from wishlist.";
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [ActionName("User")]
    [AllowAnonymous]
    public async Task<IActionResult> Member(Guid userId)
    {
        var response = await _apiClient.GetAsync<PublicUserProfileDTO>($"Profile/GetPublicUserProfile/{userId}");

        if (response == null || !response.IsSuccess || response.Data == null)
        {
            TempData["Error"] = response?.Message ?? "User profile not found.";
            return RedirectToAction("Index", "Discover");
        }

        return View("PublicProfile", response.Data);
    }
}
