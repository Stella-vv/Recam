namespace Remp.Models.Responses;

public class CreateAgentResponse
{
    public required string Email { get; set; }
    public required string AgentId { get; set; }
    public required string PhotographyCompanyId { get; set; }
}