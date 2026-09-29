using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class PaymentTransactionDTO
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public PaymentStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
