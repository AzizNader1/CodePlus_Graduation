using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class CancelSessionRequest
{
    public string Reason { get; set; } = string.Empty;
}
