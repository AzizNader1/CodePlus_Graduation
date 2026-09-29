using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public AdminController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var statsTask = _apiClient.GetAsync<AdminDashboardStatsDTO>("Admin/GetStats");
        var reportsTask = _apiClient.GetAsync<List<ReportDTO>>("Admin/GetReports");

        await Task.WhenAll(statsTask, reportsTask);

        ViewBag.Stats = statsTask.Result?.Data ?? new AdminDashboardStatsDTO();
        var reports = reportsTask.Result?.Data ?? new List<ReportDTO>();

        return View(reports);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveReport(Guid reportId, string actionTaken, string? adminNotes)
    {
        var request = new ResolveReportRequest
        {
            Status = SkillSwap.Domain.Enums.ReportStatus.Resolved,
            AdminNotes = !string.IsNullOrEmpty(actionTaken) ? $"{actionTaken}: {adminNotes}" : (adminNotes ?? "Resolved by platform administrator.")
        };

        var response = await _apiClient.PutCommandAsync($"Admin/ResolveReport/{reportId}", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to resolve report.";
        }
        else
        {
            TempData["Success"] = "Report marked as resolved successfully.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, bool isActive)
    {
        var response = await _apiClient.PutCommandAsync($"Admin/UpdateUserStatus/{userId}?isActive={isActive}");

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to update user status.";
        }
        else
        {
            TempData["Success"] = $"User status updated to {(isActive ? "Active" : "Suspended")}.";
        }

        return RedirectToAction("Index");
    }
}
