namespace Remp.Models.Logs;

public class CaseHistoryLog
{
    public int ListingCaseId { get; set; }
    public required string UserId { get; set; }
    public required string Action { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}