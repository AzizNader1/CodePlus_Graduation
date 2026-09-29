using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public ChatController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index(Guid? conversationId)
    {
        var convosResponse = await _apiClient.GetAsync<List<ConversationDTO>>("Chat/GetConversations");
        var conversations = convosResponse?.Data ?? new List<ConversationDTO>();

        ConversationDTO? activeConvo = null;
        PaginatedList<MessageDTO>? messages = null;

        if (conversations.Any())
        {
            var targetId = conversationId ?? conversations.First().Id;
            activeConvo = conversations.FirstOrDefault(c => c.Id == targetId) ?? conversations.First();

            var msgsResponse = await _apiClient.GetAsync<PaginatedList<MessageDTO>>($"Chat/GetMessages/{activeConvo.Id}");
            messages = msgsResponse?.Data;
        }

        ViewBag.Conversations = conversations;
        ViewBag.ActiveConversation = activeConvo;
        ViewBag.Messages = messages?.Items ?? new List<MessageDTO>();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid conversationId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return RedirectToAction("Index", new { conversationId });

        var request = new SendMessageRequest { Content = content };
        var response = await _apiClient.PostAsync<MessageDTO>($"Chat/SendMessage/{conversationId}", request);

        if (response == null || !response.IsSuccess)
        {
            TempData["Error"] = response?.Message ?? "Failed to send message.";
        }

        return RedirectToAction("Index", new { conversationId });
    }
}
