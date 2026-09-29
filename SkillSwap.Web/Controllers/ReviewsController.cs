using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public ReviewsController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewRequest request)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all review details.";
            return RedirectToAction("Index", "Sessions");
        }

        var response = await _apiClient.PostAsync<ReviewDTO>("Reviews/CreateReview", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to submit review.";
        }
        else
        {
            TempData["Success"] = "Thank you! Your peer review has been published and added to their profile.";
        }

        return RedirectToAction("Index", "Sessions");
    }
}
