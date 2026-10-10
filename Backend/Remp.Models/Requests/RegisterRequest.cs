using System.ComponentModel.DataAnnotations;

namespace Remp.Models.Requests;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email {get; set;} ="";

    [Required]
    public string Password {get; set;} ="";

    [Required]
    [Compare(nameof(Password), ErrorMessage = "passwords do not match.")]
    public string ConfirmPassword {get; set;} ="";

    [Required]
    [StringLength(100)]
    public string AgentFirstName {get; set;} ="";

    [Required]
    [StringLength(100)]
    public string AgentLastName {get; set;} ="";

    [StringLength(200)]
    public string? CompanyName {get; set;}
}