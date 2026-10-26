namespace Remp.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Responses;

public interface IUserRepository
{
    Task<int> CountActiveUsersAsync();

    public Task<List<UserResponse>> GetActiveUsersPageAsync(int skipCount, int pageSize);
}