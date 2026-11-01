using Remp.Models.Responses;
using Remp.Models.Requests;
using Remp.Models.Results;

namespace Remp.API.Services.Auth;

public interface IAuthService
{
    public Task<LoginResponse?> LoginAsync(LoginRequest request);
    public Task<RegisterResult> RegisterAsync(RegisterRequest request);
}