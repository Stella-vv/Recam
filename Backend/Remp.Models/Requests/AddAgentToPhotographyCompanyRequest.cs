using System.ComponentModel.DataAnnotations;

namespace Remp.Models.Requests;

public class AddAgentToPhotographyCompanyRequest
{
    [Required]
    public string AgentId {get; set;} = "";
}