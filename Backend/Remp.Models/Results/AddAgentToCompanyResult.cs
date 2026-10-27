using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class AddAgentToCompanyResult
{
    public AddAgentToCompanyStatus Status {get; set;}
    public AddAgentToPhotographyCompanyResponse? Data {get; set;}
}