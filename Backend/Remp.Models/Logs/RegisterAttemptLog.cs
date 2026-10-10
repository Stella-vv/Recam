using System.Reflection.Metadata;

namespace Remp.Models.Logs;

public class RegisterAttemptLog
{
    public string Id {get; set;} = Guid.NewGuid().ToString();
    public string Email {get; set;} = "";
    public string? UserId {get; set;}
    public bool IsSuccessful {get; set;} = false;
    public string FailureReason {get; set;} = "";
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

}