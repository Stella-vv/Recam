using Remp.Models.Entities;
using Remp.Models.Responses;
using Remp.Models.Requests;

namespace Remp.API.Services.Auth;

public interface IAuthService
{
    public Task<LoginResponse?> LoginAsync(LoginRequest request);
}