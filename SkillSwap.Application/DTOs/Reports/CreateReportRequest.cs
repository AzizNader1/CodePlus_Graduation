using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class CreateReportRequest
{
    public Guid ReportedUserId { get; set; }
    public Guid? SessionId { get; set; }
    public string ReasonCategory { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
