using Remp.Models.Enums;

namespace Remp.Models.Entities;

public class Agent
{
    public required string Id { get; set; }

    public required string AgentFirstName { get; set; }

    public required string AgentLastName { get; set; }

    public string? AvatarUrl { get; set; }

    public string? CompanyName { get; set; }

    public ICollection<AgentListingCase> AgentListingCases { get; set; } = new List<AgentListingCase>();

    public ICollection<AgentPhotographyCompany> AgentPhotographyCompanies { get; set; } = new List<AgentPhotographyCompany>();

    public ApplicationUser? User { get; set; }
}