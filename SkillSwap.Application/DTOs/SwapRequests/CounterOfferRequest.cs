using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class CounterOfferRequest
{
    public DateTime NewProposedDate { get; set; }
    public int NewDurationMinutes { get; set; } = 60;
    public string? CounterNotes { get; set; }
}
