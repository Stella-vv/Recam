using Remp.DataAccess.Repositories;
using Remp.Models.Enums;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Remp.Models.Results;
using Microsoft.AspNetCore.Identity;
using Remp.Models.Entities;
using System.Security.Cryptography;
using Remp.API.Services.Email;
using Remp.Models.Logs;

namespace Remp.API.Services.PhotographyCompanies;

public class PhotographyCompanyService : IPhotographyCompanyService
{
    private readonly IPhotographyCompanyRepository _photographyCompanyRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<PhotographyCompanyService> _logger;
    private readonly ICreateAgentLogRepository _createAgentLogRepository;
    public PhotographyCompanyService(IPhotographyCompanyRepository photographyCompanyRepository, UserManager<ApplicationUser> userManager, IRegistrationRepository registrationRepository, IEmailSender emailSender, ILogger<PhotographyCompanyService> logger, ICreateAgentLogRepository createAgentLogRepository)
    {
      _photographyCompanyRepository = photographyCompanyRepository;
      _userManager = userManager;
      _registrationRepository = registrationRepository;
      _emailSender = emailSender;
      _logger = logger;
      _createAgentLogRepository = createAgentLogRepository;
    }

    public async Task<AddAgentToCompanyResult> AddAgentAsync(string userId, AddAgentToPhotographyCompanyRequest request)
    {
        bool companyExists = await _photographyCompanyRepository.CompanyExistsAsync(userId);
        if(!companyExists)
        {
            AddAgentToCompanyResult result = new AddAgentToCompanyResult()
            {
                Status = AddAgentToCompanyStatus.CompanyNotFound,
                Data = null
            };
            return result;
        }
        bool agentExists = await _photographyCompanyRepository.ActiveAgentExistsAsync(request.AgentId);
        if (!agentExists)
        {
            AddAgentToCompanyResult result = new AddAgentToCompanyResult()
            {
                Status = AddAgentToCompanyStatus.AgentNotFound,
                Data = null
            };
            return result;
        }

        bool relationExists = await _photographyCompanyRepository.AgentCompanyRelationExistsAsync(request.AgentId, userId);
        if (relationExists)
        {
            AddAgentToCompanyResult result = new AddAgentToCompanyResult()
            {
                Status = AddAgentToCompanyStatus.AlreadyAssociated,
                Data = null
            };
            return result;
        }

        bool added = await _photographyCompanyRepository.AddAgentCompanyRelationAsync(request.AgentId, userId);
        if (!added)
        {
            AddAgentToCompanyResult result = new AddAgentToCompanyResult()
            {
                Status = AddAgentToCompanyStatus.AlreadyAssociated,
                Data = null
            };
            return result;
        }

        AddAgentToPhotographyCompanyResponse response = new AddAgentToPhotographyCompanyResponse()
        {
            AgentId = request.AgentId,
            PhotographyCompanyId = userId
        };

        AddAgentToCompanyResult successResult = new AddAgentToCompanyResult()
        {
            Status = AddAgentToCompanyStatus.Success,
            Data = response
        };
        return successResult;

    }

    public async Task<CreateAgentResult> CreateAgentAsync(string userId, CreateAgentRequest request)
    {
        string email = request.Email.Trim();
        bool companyExists = await _photographyCompanyRepository.CompanyExistsAsync(userId);
        if (!companyExists)
        {

            CreateAgentLog companyNotFoundLog = new CreateAgentLog()
            {
                PhotographyCompanyId = userId,
                Email = email,
                IsSuccessful = false,
                EmailSent = false,
                FailureReason = "CompanyNotFound"
            };
            await SaveCreateAgentLogAsync(companyNotFoundLog);

            CreateAgentResult result = new CreateAgentResult()
            {
                Status = CreateAgentStatus.CompanyNotFound,
                Data = null
            };
            return result;
        }

        ApplicationUser? existingUser = await _userManager.FindByEmailAsync(email);
        if(existingUser != null)
        {
            CreateAgentLog duplicateEmailLog = new CreateAgentLog()
            {
                PhotographyCompanyId = userId,
                Email = email,
                IsSuccessful = false,
                EmailSent = false,
                FailureReason = "EmailAlreadyExists"
            };
            await SaveCreateAgentLogAsync(duplicateEmailLog);

            CreateAgentResult result = new CreateAgentResult()
            {
                Status = CreateAgentStatus.EmailAlreadyExists,
                Data = null
            };
            return result;
        }

        string password = GenerateRandomPassword();

        await using var transaction = await _registrationRepository.BeginTransactionAsync();
        ApplicationUser user = new ApplicationUser()
        {
            UserName = email,
            Email = email,
            IsDeleted = false
        };

        IdentityResult createResult = await _userManager.CreateAsync(user, password);
        if(!createResult.Succeeded)
        {
            await transaction.RollbackAsync();

            CreateAgentLog userCreationFailureLog = new CreateAgentLog()
            {
                PhotographyCompanyId = userId,
                Email = email,
                IsSuccessful = false,
                EmailSent = false,
                FailureReason = "UserCreationFailed"
            };
            await SaveCreateAgentLogAsync(userCreationFailureLog);

            string errors = string.Join("; ", createResult.Errors.Select(error => error.Description));
            CreateAgentResult result = new CreateAgentResult()
            {
                Status = CreateAgentStatus.UserCreationFailed,
                Data = null,
                ErrorMessage = errors
            };
            return result;
        }

        IdentityResult roleResult = await _userManager.AddToRoleAsync(user, "user");
        if (!roleResult.Succeeded)
        {
            await transaction.RollbackAsync();

            CreateAgentLog roleFailureLog = new CreateAgentLog()
            {
                PhotographyCompanyId = userId,
                Email = email,
                IsSuccessful = false,
                EmailSent = false,
                FailureReason = "RoleAssignmentFailed"
            };
            await SaveCreateAgentLogAsync(roleFailureLog);

            string errors = string.Join("; ", roleResult.Errors.Select(error => error.Description));
            CreateAgentResult result = new CreateAgentResult()
            {
                Status = CreateAgentStatus.RoleAssignmentFailed,
                Data = null,
                ErrorMessage = errors
            };
            return result;
        }

        Agent agent = new Agent()
        {
            Id = user.Id,
            AgentFirstName = request.AgentFirstName.Trim(),
            AgentLastName = request.AgentLastName.Trim(),
            CompanyName = request.CompanyName?.Trim()
        };

        await _registrationRepository.AddAgentAsync(agent);
        bool added = await _photographyCompanyRepository.AddAgentCompanyRelationAsync(agent.Id, userId);
        if(!added)
        {
            await transaction.RollbackAsync();

            CreateAgentLog associationFailureLog = new CreateAgentLog()
            {
                PhotographyCompanyId = userId,
                Email = email,
                IsSuccessful = false,
                EmailSent = false,
                FailureReason = "CompanyAssociationFailed"
            };
            await SaveCreateAgentLogAsync(associationFailureLog);

            CreateAgentResult result = new CreateAgentResult(){
                Status = CreateAgentStatus.CompanyAssociationFailed,
                ErrorMessage = "Failed to associate agent with photography company."
            };
            return result;
        }

        await transaction.CommitAsync();

        CreateAgentResponse response = new CreateAgentResponse()
        {
            Email = email,
            AgentId = agent.Id,
            PhotographyCompanyId = userId
        };

        try{
            await _emailSender.SendEmailAsync("Your Recam account",
                                              $"Your Recam account has been created.\nEmail: {email}\nPassword: {password}",
                                              email);
        }
        catch(Exception ex){
            _logger.LogError(ex, "Failed to send account email for agent {AgentId}.", agent.Id);

            CreateAgentLog emailFailureLog = new CreateAgentLog()
            {
                PhotographyCompanyId = userId,
                AgentId	= agent.Id,
                Email = email,
                IsSuccessful = true,
                EmailSent = false,
                FailureReason = "EmailSendingFailed"
            };
            await SaveCreateAgentLogAsync(emailFailureLog);

            CreateAgentResult result = new CreateAgentResult()
            {
                Status = CreateAgentStatus.EmailSendingFailed,
                Data = response,
                ErrorMessage = "Account created, but the login email could not be sent."
            };
            return result;
        }

        CreateAgentLog log = new CreateAgentLog()
        {
                PhotographyCompanyId = userId,
                AgentId	= agent.Id,
                Email = email,
                IsSuccessful = true,
                EmailSent = true
        };

        await  SaveCreateAgentLogAsync(log);

        CreateAgentResult successResult = new CreateAgentResult()
        {
            Status = CreateAgentStatus.Success,
            Data = response
        };
        return successResult;
    }

    private static string GenerateRandomPassword()
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialCharacters = "!@#$%^&*";
        string allCharacters = uppercase + lowercase + digits + specialCharacters;
        char[] password = new char[16];
        password[0] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
        password[1] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
        password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        password[3] = specialCharacters[RandomNumberGenerator.GetInt32(specialCharacters.Length)];

        for(int i = 4; i < password.Length; i++)
        {
            password[i] = allCharacters[RandomNumberGenerator.GetInt32(allCharacters.Length)];
        }

        for(int i = password.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            char temp = password[i];
            password[i] = password[j];
            password[j] = temp;
        }

        return new string(password);
    }

    private async Task SaveCreateAgentLogAsync(CreateAgentLog log)
    {
        try
        {
            await _createAgentLogRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save create-agent audit log.");
        }
    }

}
