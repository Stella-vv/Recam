using System.Dynamic;

namespace Remp.Models.Logs;

public class LoginAttemptLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = "";
    public string FailureReason { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsSuccessful { get; set; }
}