namespace Remp.DataAccess.Repositories;
using Remp.Models.Responses;

public interface IUserRepository
{
    public Task<int> CountActiveUsersAsync();

    public Task<List<UserResponse>> GetActiveUsersPageAsync(int skipCount, int pageSize);

    public Task<bool> ActiveUserExistsAsync(string userId);

    public Task<List<int>> GetAssignedListingIdsAsync(string userId);
}