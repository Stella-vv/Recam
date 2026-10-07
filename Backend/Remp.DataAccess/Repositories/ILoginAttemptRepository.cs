using Remp.Models.Logs;
namespace Remp.DataAccess.Repositories;

public interface ILoginAttemptRepository
{
  public Task AddAsync (LoginAttemptLog log);
}