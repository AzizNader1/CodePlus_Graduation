using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class ResolveReportRequest
{
    public ReportStatus Status { get; set; } = ReportStatus.Resolved;
    public string? AdminNotes { get; set; }
}
