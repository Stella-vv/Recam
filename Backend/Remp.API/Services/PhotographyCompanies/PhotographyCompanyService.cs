using Remp.DataAccess.Repositories;
using Remp.Models.Enums;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Remp.Models.Results;

namespace Remp.API.Services.PhotographyCompanies;

public class PhotographyCompanyService : IPhotographyCompanyService
{
    private readonly IPhotographyCompanyRepository _photographyCompanyRepository;
    public PhotographyCompanyService(IPhotographyCompanyRepository photographyCompanyRepository)
    {
      _photographyCompanyRepository = photographyCompanyRepository;
    }

    public async Task<AddAgentToCompanyResult> AddAgentAsync(string userId, AddAgentToPhotographyCompanyRequest request)
    {
        bool companyExists = await _photographyCompanyRepository.CompanyExistsAsync(userId);
        if(!companyExists)
        {
            AddAgentToCompanyResult result = new AddAgentToCompanyResult()
            {
                Status = AddAgentToCompanyStatus.CompanyNotFound,
                Data = null
            };
            return result;
        }
        bool agentExists = await _photographyCompanyRepository.ActiveAgentExistsAsync(request.AgentId);
        if (!agentExists)
        {
            AddAgentToCompanyResult result = new AddAgentToCompanyResult()
            {
                Status = AddAgentToCompanyStatus.AgentNotFound,
                Data = null
            };
            return result;
        }

        bool relationExists = await _photographyCompanyRepository.AgentCompanyRelationExistsAsync(request.AgentId, userId);
        if (relationExists)
        {
            AddAgentToCompanyResult result = new AddAgentToCompanyResult()
            {
                Status = AddAgentToCompanyStatus.AlreadyAssociated,
                Data = null
            };
            return result;
        }

        bool added = await _photographyCompanyRepository.AddAgentCompanyRelationAsync(request.AgentId, userId);
        if (!added)
        {
            AddAgentToCompanyResult result = new AddAgentToCompanyResult()
            {
                Status = AddAgentToCompanyStatus.AlreadyAssociated,
                Data = null
            };
            return result;
        }

        AddAgentToPhotographyCompanyResponse response = new AddAgentToPhotographyCompanyResponse()
        {
            AgentId = request.AgentId,
            PhotographyCompanyId = userId
        };

        AddAgentToCompanyResult successResult = new AddAgentToCompanyResult()
        {
            Status = AddAgentToCompanyStatus.Success,
            Data = response
        };
        return successResult;

    }
}
