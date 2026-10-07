using Microsoft.AspNetCore.Mvc;
namespace Remp.API.Controllers;
using Microsoft.AspNetCore.Identity;
using Remp.Models.Entities;
using Remp.API.Services.Auth;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Remp.DataAccess.Repositories;
using Remp.Models.Logs;

[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILoginAttemptRepository _loginAttemptRepository;

    public AuthController(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService, ILoginAttemptRepository loginAttemptRepository){
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _loginAttemptRepository = loginAttemptRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
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
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid email or password."));
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
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid email or password."));
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

        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }

    [Authorize]
    [HttpGet("test-auth")]
    public IActionResult TestAuth()
    {
        return Ok(ApiResponse<string>.Ok("Token is valid."));
    }

    [Authorize(Roles = "photographyCompany")]
    [HttpGet("test-company-role")]
    public IActionResult TestCompanyRole()
    {
        return Ok(ApiResponse<string>.Ok("Photography company access granted."));
    }
}