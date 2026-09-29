using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

[Authorize]
public class SessionsController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public SessionsController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(SessionStatus? status)
    {
        var endpoint = status.HasValue 
            ? $"Sessions/GetSessions?status={status.Value}" 
            : "Sessions/GetSessions";

        var response = await _apiClient.GetAsync<List<SwapSessionDTO>>(endpoint);
        var sessions = response?.Data ?? new List<SwapSessionDTO>();
        ViewBag.SelectedStatus = status;
        ViewBag.Message = response?.Message;

        return View(sessions);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid sessionId)
    {
        var response = await _apiClient.GetAsync<SwapSessionDTO>($"Sessions/GetById/{sessionId}");

        if (response == null || !response.IsSuccess || response.Data == null)
        {
            TempData["Error"] = response?.Message ?? "Session not found.";
            return RedirectToAction("Index");
        }

        return View(response.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Room(Guid sessionId)
    {
        var sessionTask = _apiClient.GetAsync<SwapSessionDTO>($"Sessions/GetById/{sessionId}");
        var joinTask = _apiClient.PostAsync<JoinSessionCallResponseDTO>($"Sessions/Join/{sessionId}");

        await Task.WhenAll(sessionTask, joinTask);

        var session = sessionTask.Result?.Data;
        var joinInfo = joinTask.Result?.Data;

        if (session == null)
        {
            TempData["Error"] = "Unable to enter session room.";
            return RedirectToAction("Index");
        }

        ViewBag.JoinInfo = joinInfo;
        return View(session);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid sessionId)
    {
        var request = new CompleteSessionRequest { Notes = "Completed via web client classroom." };
        var response = await _apiClient.PostCommandAsync($"Sessions/Complete/{sessionId}", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to mark session completed.";
        }
        else
        {
            TempData["Success"] = "Session completion confirmed! You can now leave a review for your partner.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid sessionId, string? reason)
    {
        var request = new CancelSessionRequest { Reason = reason ?? "Cancelled by user." };
        var response = await _apiClient.PostCommandAsync($"Sessions/Cancel/{sessionId}", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to cancel session.";
        }
        else
        {
            TempData["Success"] = "Session cancelled.";
        }

        return RedirectToAction("Index");
    }
}
