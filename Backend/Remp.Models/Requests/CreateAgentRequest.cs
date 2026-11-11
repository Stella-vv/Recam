using System.ComponentModel.DataAnnotations;

namespace Remp.Models.Requests;

public class CreateAgentRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string AgentFirstName { get; set; }

    [Required]
    public required string AgentLastName { get; set; }

    public string? CompanyName { get; set; }

}