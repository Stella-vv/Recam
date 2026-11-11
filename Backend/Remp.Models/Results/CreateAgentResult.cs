using Remp.Models.Enums;
using Remp.Models.Responses;

namespace Remp.Models.Results;

public class CreateAgentResult
{
      public CreateAgentStatus Status { get; set; }
      public CreateAgentResponse? Data { get; set; }
      public string? ErrorMessage { get; set; }
}