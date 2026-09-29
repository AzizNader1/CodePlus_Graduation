using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public NotificationsController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var response = await _apiClient.GetAsync<PaginatedList<NotificationDTO>>("Notifications/GetNotifications");
        var notifications = response?.Data?.Items ?? new List<NotificationDTO>();
        ViewBag.Message = response?.Message;
        return View(notifications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        await _apiClient.PutCommandAsync($"Notifications/MarkAsRead/{notificationId}");
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var response = await _apiClient.PutCommandAsync("Notifications/MarkAllAsRead");
        TempData["Success"] = response?.Message ?? "All notifications marked as read.";
        return RedirectToAction("Index");
    }
}
