using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class ReportDTO
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public Guid ReportedUserId { get; set; }
    public string ReportedUserName { get; set; } = string.Empty;
    public Guid? SessionId { get; set; }
    public string ReasonCategory { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}
