using System.ComponentModel.DataAnnotations;

namespace Remp.Models.Requests;

public class AddCaseContactRequest
{
    [Required]
    public string FirstName { get; set; } = "";
    [Required]
    public string LastName { get; set; } = "";
    public string? CompanyName { get; set; }
    [Url]
    public string? ProfileUrl { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

}