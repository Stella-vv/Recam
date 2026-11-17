using Remp.Models.Entities;

namespace Remp.DataAccess.Repositories;

public interface IPhotographyCompanyRepository
{
    public Task<bool> CompanyExistsAsync(string companyId);
    public Task<bool> ActiveAgentExistsAsync(string agentId);
    public Task<bool> AgentCompanyRelationExistsAsync(string agentId, string companyId);
    public Task<bool> AddAgentCompanyRelationAsync(string agentId, string companyId);
    public Task<Agent?> FindActiveAgentByEmailAsync(string normalizedEmail);
    public Task<List<Agent>> GetActiveAgentsByCompanyIdAsync(string companyId);

}