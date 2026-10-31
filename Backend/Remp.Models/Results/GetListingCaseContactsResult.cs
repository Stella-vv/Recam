using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class GetListingCaseContactsResult
{
    public GetListingCaseContactsStatus Status {get; set;}
    public GetListingCaseContactsResponse? Data {get; set;}
}