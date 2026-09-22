namespace Remp.Models.Entities;

public class PhotographyCompany
{
    public required string Id { get; set; }

    public required string PhotographyCompanyName { get; set; }

    public ICollection<AgentPhotographyCompany> AgentPhotographyCompanies = new List<AgentPhotographyCompany>();

    public ApplicationUser? User { get; set; }
}