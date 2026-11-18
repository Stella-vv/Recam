using Microsoft.EntityFrameworkCore.Storage;
using Remp.Models.Entities;
using Remp.Models.Logs;

namespace Remp.DataAccess.Repositories;

public interface IUpdateAccountPasswordLogRepository
{
    public Task AddAsync(UpdateAccountPasswordLog log);
}