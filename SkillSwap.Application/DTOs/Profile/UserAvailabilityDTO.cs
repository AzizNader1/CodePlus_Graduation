using SkillSwap.Domain.Enums;


namespace SkillSwap.Application.DTOs;

public class UserAvailabilityDTO
{
    public Guid Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsRecurring { get; set; }
}
