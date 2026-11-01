using Microsoft.EntityFrameworkCore.Storage;
using Remp.Models.Entities;

namespace Remp.DataAccess.Repositories;

public interface IRegistrationRepository
{
    public Task<IDbContextTransaction> BeginTransactionAsync();
    public Task AddAgentAsync(Agent agent);
}