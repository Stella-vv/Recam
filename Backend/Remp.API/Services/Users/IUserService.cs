using Remp.Models.Requests;
using Remp.Models.Responses;
using Remp.Models.Results;

namespace Remp.API.Services.Users;

public interface IUserService
{
    public Task<GetUsersResponse> GetUsersAsync(GetUsersRequest request);
    public Task<GetCurrentUserResponse?> GetCurrentUserAsync(string userId, List<string> roles);
    public Task<UpdateAccountPasswordResult> UpdateAccountPasswordAsync(string userId, UpdateAccountPasswordRequest request);

}