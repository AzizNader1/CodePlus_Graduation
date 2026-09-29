using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;
using SkillSwap.Web.Models;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

public class DiscoverController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public DiscoverController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var feedTask = _apiClient.GetAsync<HomeFeedDTO>("Discover/GetFeed");
        var catTask = _apiClient.GetAsync<List<CategoryDTO>>("Categories/GetCategories");

        await Task.WhenAll(feedTask, catTask);

        var feed = feedTask.Result?.Data ?? new HomeFeedDTO();
        ViewBag.Categories = catTask.Result?.Data ?? new List<CategoryDTO>();

        return View(feed);
    }

    public async Task<IActionResult> Search(
        string? keyword,
        Guid? categoryId,
        ProficiencyLevel? level,
        double? minRating,
        string? location,
        int pageNumber = 1,
        int pageSize = 12)
    {
        var queryParams = new List<string>
        {
            $"pageNumber={pageNumber}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrWhiteSpace(keyword))
            queryParams.Add($"keyword={Uri.EscapeDataString(keyword)}");
        if (categoryId.HasValue)
            queryParams.Add($"categoryId={categoryId.Value}");
        if (level.HasValue)
            queryParams.Add($"level={level.Value}");
        if (minRating.HasValue)
            queryParams.Add($"minRating={minRating.Value}");
        if (!string.IsNullOrWhiteSpace(location))
            queryParams.Add($"location={Uri.EscapeDataString(location)}");

        var endpoint = $"Discover/Search?{string.Join("&", queryParams)}";
        var searchTask = _apiClient.GetAsync<PaginatedList<UserSkillDTO>>(endpoint);
        var catTask = _apiClient.GetAsync<List<CategoryDTO>>("Categories/GetCategories");

        await Task.WhenAll(searchTask, catTask);

        ViewBag.Keyword = keyword;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.SelectedLevel = level;
        ViewBag.SelectedMinRating = minRating;
        ViewBag.Location = location;
        ViewBag.Categories = catTask.Result?.Data ?? new List<CategoryDTO>();

        var result = searchTask.Result?.Data;
        return View(result);
    }

    [Authorize]
    public async Task<IActionResult> Matches()
    {
        var response = await _apiClient.GetAsync<List<SwapMatchDTO>>("Discover/GetRecommendedMatches");
        var matches = response?.Data ?? new List<SwapMatchDTO>();
        ViewBag.Message = response?.Message;
        return View(matches);
    }
}
