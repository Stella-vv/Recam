using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class UpdateAccountPasswordResult
{
    public UpdateAccountPasswordStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
}