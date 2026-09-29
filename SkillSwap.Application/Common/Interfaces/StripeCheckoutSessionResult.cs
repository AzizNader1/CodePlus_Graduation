namespace SkillSwap.Application.Common.Interfaces;

public class StripeCheckoutSessionResult
{
    public string SessionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
}
