
namespace Remp.Models.Logs;

public class CreateAgentLog
{
    public required string PhotographyCompanyId { get; set; }
    public string? AgentId { get; set; }
    public required string Email { get; set; }
    public bool IsSuccessful { get; set; }
    public bool EmailSent { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}