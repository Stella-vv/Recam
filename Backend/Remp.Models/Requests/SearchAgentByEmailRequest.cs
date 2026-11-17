using System.ComponentModel.DataAnnotations;

namespace Remp.Models.Requests;

public class SearchAgentByEmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

}