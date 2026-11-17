using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class GetPhotographyCompanyAgentsResult
{
    public 	GetPhotographyCompanyAgentsStatus Status { get; set; }
    public GetPhotographyCompanyAgentsResponse? Data { get; set; }
}