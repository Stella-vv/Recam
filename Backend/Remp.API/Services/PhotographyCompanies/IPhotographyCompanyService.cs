using Remp.Models.Results;
using Remp.Models.Requests;

namespace Remp.API.Services.PhotographyCompanies;

public interface IPhotographyCompanyService
{
    public Task<AddAgentToCompanyResult> AddAgentAsync(string userId, AddAgentToPhotographyCompanyRequest request);
    public Task<CreateAgentResult> CreateAgentAsync(string userId, CreateAgentRequest request);
}