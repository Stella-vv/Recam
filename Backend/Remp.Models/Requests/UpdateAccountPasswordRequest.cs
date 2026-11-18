using System.ComponentModel.DataAnnotations;

namespace Remp.Models.Requests;

public class UpdateAccountPasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = "";
    [Required]
    public string NewPassword { get; set; } = "";
}