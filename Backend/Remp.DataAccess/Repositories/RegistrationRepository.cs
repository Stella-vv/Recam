using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace Remp.DataAccess.Repositories;

public class RegistrationRepository : IRegistrationRepository
{
    private readonly ApplicationDbContext _context;

    public RegistrationRepository (ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _context.Database.BeginTransactionAsync();
    }
    public async Task AddAgentAsync(Agent agent)
    {
        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();
    }
}