using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Remp.Models.Entities;
using Remp.API.Services.Auth;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Remp.DataAccess.Repositories;
using Remp.DataAccess.Data;
using Remp.Models.Results;
using Remp.Models.Enums;

namespace Remp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService){
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        LoginResponse? response = await _authService.LoginAsync(request);
        if(response == null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid email or password."));
        }
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

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {

        RegisterResult result = await _authService.RegisterAsync(request);

        if(result.Status == RegisterStatus.EmailAlreadyExists)
        {
            return Conflict(ApiResponse<RegisterResponse>.Fail("Email is already registered."));
        }
        else if(result.Status == RegisterStatus.UserCreationFailed)
        {
            return BadRequest(ApiResponse<RegisterResponse>.Fail(result.ErrorMessage ?? "Registration failed."));
        }
        else if(result.Status == RegisterStatus.RoleAssignmentFailed)
        {
            return StatusCode(500, ApiResponse<RegisterResponse>.Fail("Registration failed. Please try again."));
        }

        return StatusCode(201, ApiResponse<RegisterResponse>.Ok(result.Data!));
    }
}