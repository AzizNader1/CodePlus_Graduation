namespace SkillSwap.Application.Common.Interfaces;

public interface IStripeService
{
    Task<StripeCheckoutSessionResult> CreateDepositCheckoutSessionAsync(Guid userId, decimal amount, string successUrl, string cancelUrl);
    Task<StripeCheckoutSessionResult> CreateSubscriptionCheckoutSessionAsync(Guid userId, string planPriceId, string successUrl, string cancelUrl);
    Task<bool> VerifyWebhookSignatureAsync(string jsonPayload, string stripeSignatureHeader, string webhookSecret);
}
