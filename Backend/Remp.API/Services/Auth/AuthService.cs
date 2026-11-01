using Microsoft.AspNetCore.Identity;
using Remp.DataAccess.Repositories;
using Remp.Models.Entities;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Remp.Models.Logs;
using Remp.Models.Enums;
using Remp.Models.Results;

namespace Remp.API.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser>	_userManager;
    private readonly IJwtTokenService	_jwtTokenService;
    private readonly ILoginAttemptRepository	_loginAttemptRepository;
    private readonly IRegisterAttemptRepository	_registerAttemptRepository;
    private readonly ILogger<AuthService>	_logger;
    private readonly IRegistrationRepository _registrationRepository;

    public AuthService(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService, ILoginAttemptRepository loginAttemptRepository,
                       IRegisterAttemptRepository	registerAttemptRepository, ILogger<AuthService>	logger, IRegistrationRepository registrationRepository)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _loginAttemptRepository = loginAttemptRepository;
        _logger = logger;
        _registerAttemptRepository = registerAttemptRepository;
        _registrationRepository = registrationRepository;
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

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        string email = request.Email.Trim();
        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
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

            RegisterResult result = new RegisterResult()
            {
                Status = RegisterStatus.EmailAlreadyExists
            };

            return result;
        }

        await using var transaction = await _registrationRepository.BeginTransactionAsync();

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

            RegisterResult result = new RegisterResult()
            {
                Status = RegisterStatus.UserCreationFailed,
                ErrorMessage = errors
            };
            return result;
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

            RegisterResult result = new RegisterResult()
            {
                Status = RegisterStatus.RoleAssignmentFailed
            };
            return result;
        }

        Agent agent = new Agent()
        {
            Id = user.Id,
            AgentFirstName = request.AgentFirstName.Trim(),
            AgentLastName = request.AgentLastName.Trim(),
            CompanyName= request.CompanyName?.Trim()
        };

        await _registrationRepository.AddAgentAsync(agent);
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

        RegisterResult successResult = new RegisterResult()
        {
            Status = RegisterStatus.Success,
            Data = response
        };

        return successResult;

    }
  }