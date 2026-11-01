using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class RegisterResult
{
    public 	RegisterStatus Status { get; set; }
    public 	RegisterResponse? Data { get; set; }
    public 	string? ErrorMessage { get; set; }

}