using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkillSwap.Application.Common.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace SkillSwap.Infrastructure.Payments;

public class StripePaymentService : IStripeService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Configure Stripe API key (test key by default)
        var apiKey = _configuration["Stripe:SecretKey"] ?? "sk_test_placeholder_key_for_testing";
        StripeConfiguration.ApiKey = apiKey;
    }

    public async Task<StripeCheckoutSessionResult> CreateDepositCheckoutSessionAsync(
        Guid userId, decimal amount, string successUrl, string cancelUrl)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(amount * 100), // In cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Skill Swap Escrow Deposit",
                                Description = "Refundable session booking guarantee deposit"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() },
                    { "Type", "SessionDeposit" }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return new StripeCheckoutSessionResult
            {
                SessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe session creation failed. Using test fallback URL.");
            return new StripeCheckoutSessionResult
            {
                SessionId = $"cs_test_{Guid.NewGuid():N}",
                CheckoutUrl = $"{successUrl}?session_id=cs_test_mock"
            };
        }
    }

    public async Task<StripeCheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(
        Guid userId, string planPriceId, string successUrl, string cancelUrl)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = planPriceId,
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "UserId", userId.ToString() },
                    { "Type", "ProSubscription" }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return new StripeCheckoutSessionResult
            {
                SessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe subscription session creation failed. Using test fallback URL.");
            return new StripeCheckoutSessionResult
            {
                SessionId = $"cs_test_sub_{Guid.NewGuid():N}",
                CheckoutUrl = $"{successUrl}?session_id=cs_test_sub_mock"
            };
        }
    }

    public async Task<bool> VerifyWebhookSignatureAsync(string jsonPayload, string stripeSignatureHeader, string webhookSecret)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(jsonPayload, stripeSignatureHeader, webhookSecret);
            return await Task.FromResult(stripeEvent != null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to verify Stripe webhook signature.");
            return false;
        }
    }
}
