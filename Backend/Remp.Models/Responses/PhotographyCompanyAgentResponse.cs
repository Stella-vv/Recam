namespace Remp.Models.Responses;

public class PhotographyCompanyAgentResponse
{
    public string AgentId { get; set; } = "";
    public string Email { get; set; } = "";
    public string AgentFirstName { get; set; } = "";
    public string AgentLastName { get; set; } = "";
    public string? CompanyName { get; set; }
    public string? AvatarUrl { get; set; }
}