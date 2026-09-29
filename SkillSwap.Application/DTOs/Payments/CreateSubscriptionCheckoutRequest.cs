using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class CreateSubscriptionCheckoutRequest
{
    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.ProMonthly;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
