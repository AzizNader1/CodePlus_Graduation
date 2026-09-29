using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Enums;
using SkillSwap.Web.Services;

namespace SkillSwap.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly ISkillSwapApiClient _apiClient;

    public PaymentsController(ISkillSwapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IActionResult> Index()
    {
        var response = await _apiClient.GetAsync<List<PaymentTransactionDTO>>("Payments/GetPaymentHistory");
        var history = response?.Data ?? new List<PaymentTransactionDTO>();
        ViewBag.Message = response?.Message;
        return View(history);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDeposit(decimal amount)
    {
        if (amount <= 0)
        {
            TempData["Error"] = "Deposit amount must be greater than zero.";
            return RedirectToAction("Index");
        }

        var request = new CreateDepositCheckoutRequest { Amount = amount };
        var response = await _apiClient.PostAsync<StripeCheckoutSessionResult>("Payments/CreateDepositCheckout", request);

        if (response == null || !response.IsSuccess || response.Data == null)
        {
            TempData["Error"] = response?.Message ?? "Failed to initiate Stripe checkout.";
            return RedirectToAction("Index");
        }

        return Redirect(response.Data.CheckoutUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSubscription(string planType = "ProMonthly")
    {
        var plan = Enum.TryParse<SubscriptionPlan>(planType, true, out var parsedPlan) ? parsedPlan : SubscriptionPlan.ProMonthly;
        var request = new CreateSubscriptionCheckoutRequest { Plan = plan };
        var response = await _apiClient.PostAsync<StripeCheckoutSessionResult>("Payments/CreateSubscriptionCheckout", request);

        if (response == null || !response.IsSuccess || response.Data == null)
        {
            TempData["Error"] = response?.Message ?? "Failed to initiate subscription checkout.";
            return RedirectToAction("Index");
        }

        return Redirect(response.Data.CheckoutUrl);
    }
}
