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
            TempData["Error"] = response?.Message ?? "We couldn't send your swap proposal. Please check the details and try again.";
            return RedirectToAction("Create", new { recipientId = request.ReceiverId });
        }

        TempData["Success"] = "Your swap request was sent successfully! You can track its progress below.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(Guid swapRequestId)
    {
        var response = await _apiClient.PostCommandAsync($"SwapRequests/Accept/{swapRequestId}");

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "We couldn't accept this swap request right now. Please try again.";
        }
        else
        {
            TempData["Success"] = "Swap request accepted! Your learning session has been automatically scheduled.";
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid swapRequestId, string? reason)
    {
        var request = new RejectRequest { Reason = reason ?? "Unable to accommodate proposal at this time." };
        var response = await _apiClient.PostCommandAsync($"SwapRequests/Reject/{swapRequestId}", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "We couldn't decline this swap request right now. Please try again.";
        }
        else
        {
            TempData["Success"] = "The swap request has been declined.";
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
            TempData["Error"] = response?.Message ?? "We couldn't send your counter proposal right now. Please try again.";
        }
        else
        {
            TempData["Success"] = "Your proposed changes have been sent to your learning partner!";
        }

        return RedirectToAction("Index");
    }
}
