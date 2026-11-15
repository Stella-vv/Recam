using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class SearchAgentByEmailResult
{
    public SearchAgentByEmailStatus Status { get; set; }
    public SearchAgentByEmailResponse? Data { get; set; }

}