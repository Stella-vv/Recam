using Microsoft.AspNetCore.Identity;
using Remp.DataAccess.Repositories;
using Remp.Models.Entities;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Remp.Models.Logs;

namespace Remp.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser>	_userManager;
    private readonly IJwtTokenService	_jwtTokenService;
    private readonly ILoginAttemptRepository	_loginAttemptRepository;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService, ILoginAttemptRepository loginAttemptRepository)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _loginAttemptRepository = loginAttemptRepository;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        ApplicationUser? user = await _userManager.FindByEmailAsync(request.Email);
        if ( user == null || user.IsDeleted)
        {
            LoginAttemptLog log = new LoginAttemptLog()
            {
              Email = request.Email,
              FailureReason = "Invalid email or deleted user",
              IsSuccessful = false,
            };
            await _loginAttemptRepository.AddAsync(log);
            return null;
        }

        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            LoginAttemptLog log = new LoginAttemptLog()
            {
                Email = request.Email,
                FailureReason = "InvalidPassword",
                IsSuccessful = false,
            };
            await _loginAttemptRepository.AddAsync(log);
            return null;
        }

        IList<string> roles = await _userManager.GetRolesAsync(user);
        LoginResponse response = _jwtTokenService.GenerateToken(user, roles);
        LoginAttemptLog successLog = new LoginAttemptLog()
        {
            Email = request.Email,
            IsSuccessful = true,
            FailureReason = ""
        };

        await _loginAttemptRepository.AddAsync(successLog);

        return response;
    }
  }