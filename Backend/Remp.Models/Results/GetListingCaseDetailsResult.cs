using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class GetListingCaseDetailsResult
{
    public GetListingCaseDetailsStatus Status { get; set; }
    public GetListingCaseDetailsResponse? Data { get; set; }
}