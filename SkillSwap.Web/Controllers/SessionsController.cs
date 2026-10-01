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
            TempData["Error"] = response?.Message ?? "We couldn't locate this session. It may have been completed or cancelled.";
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
            TempData["Error"] = "We couldn't open the video call room. Please ensure your session is active and try again.";
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
            TempData["Error"] = response?.Message ?? "We couldn't confirm completion for this session. Please try again.";
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
            TempData["Error"] = response?.Message ?? "We couldn't cancel this session right now. Please try again.";
        }
        else
        {
            TempData["Success"] = "Your session has been cancelled.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Heartbeat(Guid sessionId, [FromBody] SessionHeartbeatRequest? request)
    {
        var response = await _apiClient.PostCommandAsync($"Sessions/Heartbeat/{sessionId}", request ?? new SessionHeartbeatRequest());
        return Json(new { success = response?.IsSuccess ?? false });
    }
}
