using Remp.Models.Logs;

namespace Remp.DataAccess.Repositories;

public interface ICreateAgentLogRepository
{
    public Task AddAsync(CreateAgentLog log);
}