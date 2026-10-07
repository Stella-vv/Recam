namespace Remp.API.Services.Auth;
using Remp.Models.Entities;
using Remp.Models.Responses;

public interface IJwtTokenService
{
    public LoginResponse GenerateToken(ApplicationUser user, IList<string> roles);
}