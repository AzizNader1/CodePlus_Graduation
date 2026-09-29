using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class CreateDepositCheckoutRequest
{
    public decimal Amount { get; set; } = 10.00m;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}
