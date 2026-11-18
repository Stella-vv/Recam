namespace Remp.Models.Logs;

public class UpdateAccountPasswordLog
{
    public string UserId { get; set; } = "";
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}