using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class ChangeListingCaseStatusResult
{
    public ChangeListingCaseStatusResultStatus Status { get; set; }
    public ChangeListingCaseStatusResponse? Data { get; set; }
}