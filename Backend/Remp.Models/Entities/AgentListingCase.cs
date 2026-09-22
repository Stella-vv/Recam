namespace Remp.Models.Entities;

public class AgentListingCase
{
    public required string AgentId { get; set; }
    
    public int ListingCaseId { get; set; }

    public Agent? Agent { get; set; }

    public ListingCase? ListingCase { get; set; }

}