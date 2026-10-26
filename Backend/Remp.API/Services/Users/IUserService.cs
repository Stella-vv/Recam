using Remp.Models.Requests;
using Remp.Models.Responses;

namespace Remp.API.Services.Users;

public interface IUserService
{
    public Task<GetUsersResponse> GetUsersAsync(GetUsersRequest request);
    public Task<GetCurrentUserResponse?> GetCurrentUserAsync(string userId, List<string> roles);

}