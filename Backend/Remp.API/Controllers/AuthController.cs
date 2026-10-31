using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Remp.Models.Entities;
using Remp.API.Services.Auth;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Remp.DataAccess.Repositories;
using Remp.Models.Logs;
using Remp.DataAccess.Data;

namespace Remp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRegisterAttemptRepository _registerAttemptRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(UserManager<ApplicationUser> userManager, IRegisterAttemptRepository registerAttemptRepository,
                           ApplicationDbContext context, ILogger<AuthController> logger, IAuthService authService){
        _userManager = userManager;
        _registerAttemptRepository = registerAttemptRepository;
        _context = context;
        _logger = logger;
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
        string email = request.Email.Trim();
        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(email);
        if ( existingUser != null)
        {
            RegisterAttemptLog log = new RegisterAttemptLog()
            {
                Email = email,
                IsSuccessful = false,
                FailureReason = "EmailAlreadyExists",
            };

            try
            {
                await _registerAttemptRepository.AddAsync(log);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save registration failure audit log.");
            }

            return Conflict(ApiResponse<RegisterResponse>.Fail("Email is already registered."));
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        ApplicationUser user = new ApplicationUser(){
            UserName = email,
            Email = email,
            IsDeleted = false
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            await transaction.RollbackAsync();
            RegisterAttemptLog log = new RegisterAttemptLog()
            {
                Email = email,
                IsSuccessful = false,
                FailureReason = "UserCreationFailed"
            };

            try
            {
                await _registerAttemptRepository.AddAsync(log);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save registration failure audit log.");
            }

            string errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
            return BadRequest(ApiResponse<RegisterResponse>.Fail(errors));
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, "user");

        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync();
            RegisterAttemptLog log = new RegisterAttemptLog()
            {
                Email = email,
                IsSuccessful = false,
                FailureReason = "RoleAssignmentFailed"
            };

            try
            {
                await _registerAttemptRepository.AddAsync(log);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save registration failure audit log.");
            }

            return StatusCode(500, ApiResponse<RegisterResponse>.Fail("Registration failed. Please try again."));
        }

        Agent agent = new Agent()
        {
            Id = user.Id,
            AgentFirstName = request.AgentFirstName.Trim(),
            AgentLastName = request.AgentLastName.Trim(),
            CompanyName= request.CompanyName?.Trim()
        };
        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        try
        {
            RegisterAttemptLog log = new RegisterAttemptLog()
            {
                Email = email,
                UserId = user.Id,
                IsSuccessful= true
            };
            await _registerAttemptRepository.AddAsync(log);
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to save registration audit log for user {UserId}.", user.Id);
        }

        RegisterResponse response = new RegisterResponse()
        {
            UserId = user.Id,
            Email= email,
            Role= "user"
        };
        return StatusCode(201, ApiResponse<RegisterResponse>.Ok(response));
    }





}