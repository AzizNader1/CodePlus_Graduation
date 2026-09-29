using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

public class HomeController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ISkillSwapApiClient apiClient, ILogger<HomeController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var response = await _apiClient.GetAsync<HomeFeedDTO>("Discover/GetFeed");
        var model = response?.Data ?? new HomeFeedDTO();
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
