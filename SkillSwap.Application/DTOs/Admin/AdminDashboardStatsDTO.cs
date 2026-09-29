using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class AdminDashboardStatsDTO
{
    public int TotalRegisteredUsers { get; set; }
    public int ActiveSwapsCount { get; set; }
    public int CompletedSessionsCount { get; set; }
    public int OpenReportsCount { get; set; }
    public int TotalSkillsListed { get; set; }
    public decimal TotalPaymentVolume { get; set; }
}
