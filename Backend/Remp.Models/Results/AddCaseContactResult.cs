using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class AddCaseContactResult
{
    public AddCaseContactStatus Status { get; set; }
    public CaseContactResponse? Data { get; set; }
}