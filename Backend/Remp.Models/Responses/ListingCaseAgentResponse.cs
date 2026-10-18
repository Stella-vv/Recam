

namespace Remp.Models.Responses;

public class ListingCaseAgentResponse
{
    public string	Id {get; set;} = "";
    public string	AgentFirstName {get; set;} = "";
    public string	AgentLastName {get; set;} = "";
    public string? AvatarUrl {get; set;}
    public string? CompanyName {get; set;}
    public string? Email {get; set;}
    public string? PhoneNumber {get; set;}
}