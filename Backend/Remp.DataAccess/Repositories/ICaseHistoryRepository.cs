using Remp.Models.Logs;

namespace Remp.DataAccess.Repositories;

public interface ICaseHistoryRepository
{
    public Task AddAsync (CaseHistoryLog log);
}