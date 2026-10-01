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
            TempData["Error"] = response?.Message ?? "We couldn't load your profile details right now. Please try again.";
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
            TempData["Error"] = "We couldn't open the profile editor right now. Please try again.";
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
            TempData["Error"] = response?.Message ?? "We couldn't save your profile changes. Please check the details and try again.";
            return RedirectToAction("Edit");
        }

        TempData["Success"] = "Your profile details have been updated successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a photo (JPG, PNG, or WebP) to upload as your profile avatar.";
            return RedirectToAction("Index");
        }

        var response = await _apiClient.PostMultipartAsync<object>("Profile/UploadAvatar", file);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "We couldn't upload your picture. Please try another image (under 5MB).";
            return RedirectToAction("Index");
        }

        TempData["Success"] = "Your profile picture has been updated!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOfferedSkill(AddOfferedSkillRequest request)
    {
        var response = await _apiClient.PostAsync<UserSkillDTO>("Skills/AddOfferedSkill", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "We couldn't add this skill to your teaching list. Please try again.";
        }
        else
        {
            TempData["Success"] = "Skill added to your teaching list!";
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
            TempData["Error"] = response?.Message ?? "We couldn't remove this skill from your profile. Please try again.";
        }
        else
        {
            TempData["Success"] = "The skill was removed from your teaching list.";
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
            TempData["Error"] = response?.Message ?? "We couldn't add this skill to your learning wishlist. Please try again.";
        }
        else
        {
            TempData["Success"] = "Skill added to your learning wishlist!";
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
            TempData["Error"] = response?.Message ?? "We couldn't remove this skill from your learning wishlist. Please try again.";
        }
        else
        {
            TempData["Success"] = "The skill was removed from your learning wishlist.";
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
            TempData["Error"] = response?.Message ?? "We couldn't find the profile you were looking for.";
            return RedirectToAction("Index", "Discover");
        }

        return View("PublicProfile", response.Data);
    }
}
