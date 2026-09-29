using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.DTOs;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

[Authorize]
public class SwapRequestsController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public SwapRequestsController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var incomingTask = _apiClient.GetAsync<List<SwapRequestDTO>>("SwapRequests/GetIncoming");
        var outgoingTask = _apiClient.GetAsync<List<SwapRequestDTO>>("SwapRequests/GetOutgoing");

        await Task.WhenAll(incomingTask, outgoingTask);

        ViewBag.Incoming = incomingTask.Result?.Data ?? new List<SwapRequestDTO>();
        ViewBag.Outgoing = outgoingTask.Result?.Data ?? new List<SwapRequestDTO>();
        ViewBag.IncomingMessage = incomingTask.Result?.Message;
        ViewBag.OutgoingMessage = outgoingTask.Result?.Message;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? recipientId, Guid? offeredSkillId, Guid? wantedSkillId)
    {
        var profileTask = _apiClient.GetAsync<UserProfileDTO>("Profile/GetProfile");
        var skillsTask = _apiClient.GetAsync<List<SkillDTO>>("Skills/GetSkills");

        await Task.WhenAll(profileTask, skillsTask);

        ViewBag.MySkills = profileTask.Result?.Data?.SkillsOffered ?? new List<UserSkillDTO>();
        ViewBag.MasterSkills = skillsTask.Result?.Data ?? new List<SkillDTO>();

        var model = new CreateSwapRequest
        {
            ReceiverId = recipientId ?? Guid.Empty,
            OfferedSkillId = offeredSkillId ?? Guid.Empty,
            RequestedSkillId = wantedSkillId ?? Guid.Empty,
            DurationMinutes = 60,
            ProposedDate = DateTime.UtcNow.AddDays(1).Date.AddHours(14)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSwapRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var response = await _apiClient.PostAsync<SwapRequestDTO>("SwapRequests/Create", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to create swap request.";
            return RedirectToAction("Create", new { recipientId = request.ReceiverId });
        }

        TempData["Success"] = "Swap request sent successfully! You can track its status below.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid swapRequestId)
    {
        var response = await _apiClient.PostCommandAsync($"SwapRequests/Accept/{swapRequestId}");

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to accept swap request.";
        }
        else
        {
            TempData["Success"] = "Swap request accepted! A swap session has been automatically scheduled.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid swapRequestId, string? reason)
    {
        var request = new RejectRequest { Reason = reason ?? "Unable to accommodate proposal." };
        var response = await _apiClient.PostCommandAsync($"SwapRequests/Reject/{swapRequestId}", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to reject swap request.";
        }
        else
        {
            TempData["Success"] = "Swap request rejected.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CounterOffer(Guid swapRequestId, DateTime newTime, int newDuration, string? counterNotes)
    {
        var request = new CounterOfferRequest
        {
            NewProposedDate = newTime,
            NewDurationMinutes = newDuration,
            CounterNotes = counterNotes
        };

        var response = await _apiClient.PostCommandAsync($"SwapRequests/CounterOffer/{swapRequestId}", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to submit counter offer.";
        }
        else
        {
            TempData["Success"] = "Counter offer submitted successfully!";
        }

        return RedirectToAction("Index");
    }
}
