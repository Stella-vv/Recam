using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class GetListingCaseMediaResult
{
    public GetListingCaseMediaStatus Status { get; set; }
    public GetListingCaseMediaResponse? Data { get; set; }
}