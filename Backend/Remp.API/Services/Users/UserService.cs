using Remp.DataAccess.Repositories;
using Remp.Models.Responses;
using Remp.Models.Requests;

namespace Remp.API.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetUsersResponse> GetUsersAsync (GetUsersRequest request)
    {
        int totalCount = await _userRepository.CountActiveUsersAsync();
        long skipCount = (long)(request.PageNumber - 1) * request.PageSize;
        List<UserResponse> users = new List<UserResponse>();
        if ( skipCount < totalCount)
        {
            users = await _userRepository.GetActiveUsersPageAsync((int)skipCount, request.PageSize);
        }

        GetUsersResponse response = new GetUsersResponse()
        {
            Users = users,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize

        };
        return response;
    }


}