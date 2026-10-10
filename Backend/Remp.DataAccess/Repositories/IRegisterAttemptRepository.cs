using Remp.Models.Logs;

namespace Remp.DataAccess.Repositories;

public interface IRegisterAttemptRepository
{
    Task AddAsync(RegisterAttemptLog log);
}