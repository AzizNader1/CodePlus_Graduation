using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class CreateSwapRequest
{
    public Guid ReceiverId { get; set; }
    public Guid OfferedSkillId { get; set; }
    public Guid RequestedSkillId { get; set; }
    public DateTime ProposedDate { get; set; }
    public int DurationMinutes { get; set; } = 60;
    public string? Notes { get; set; }
}
