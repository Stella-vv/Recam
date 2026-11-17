namespace Remp.Models.Responses;

public class GetPhotographyCompanyAgentsResponse
{
    public List<PhotographyCompanyAgentResponse> Agents { get; set; } = new List<PhotographyCompanyAgentResponse>();
}