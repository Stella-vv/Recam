using Remp.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Entities;
using Microsoft.Data.SqlClient;

namespace Remp.DataAccess.Repositories;

public class PhotographyCompanyRepository : IPhotographyCompanyRepository
{
    private readonly ApplicationDbContext _context;

    public PhotographyCompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CompanyExistsAsync(string companyId)
    {
        return await _context.PhotographyCompanies.AnyAsync(company => company.Id == companyId);
    }

    public async Task<bool> ActiveAgentExistsAsync(string agentId)
    {
        return await _context.Agents.AnyAsync(agent => agent.Id == agentId && agent.User != null && agent.User.IsDeleted == false);
    }

    public async Task<bool> AgentCompanyRelationExistsAsync(string agentId, string companyId)
    {
        return await _context.AgentPhotographyCompanies.AnyAsync(assignment => assignment.AgentId == agentId && assignment.PhotographyCompanyId == companyId);
    }

    public async Task<bool> AddAgentCompanyRelationAsync(string agentId, string companyId)
    {
        AgentPhotographyCompany relation = new AgentPhotographyCompany()
        {
            AgentId = agentId,
            PhotographyCompanyId = companyId
        };

        _context.AgentPhotographyCompanies.Add(relation);

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch(DbUpdateException ex) when(ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
        {
            _context.Entry(relation).State = EntityState.Detached;
            return false;
        }
    }

}