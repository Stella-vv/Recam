using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.Models.Requests;
using Remp.Models.Responses;
using System.Security.Claims;
using Remp.API.Services.PhotographyCompanies;
using Remp.Models.Results;
using Remp.Models.Enums;

namespace Remp.API.Controllers;


[ApiController]
[Route("api/photography-companies")]
[Authorize(Roles = "photographyCompany")]
public class PhotographyCompaniesController : ControllerBase
{
    private readonly IPhotographyCompanyService _photographyCompanyService;

    public PhotographyCompaniesController(IPhotographyCompanyService photographyCompanyService)
    {
        _photographyCompanyService = photographyCompanyService;
    }

    [HttpPost("agents")]
    public async Task<IActionResult> AddAgent(AddAgentToPhotographyCompanyRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity."));
        }

        AddAgentToCompanyResult result = await _photographyCompanyService.AddAgentAsync(userId, request);
        if(result.Status == AddAgentToCompanyStatus.CompanyNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Photography company not found."));
        }

        if(result.Status == AddAgentToCompanyStatus.AgentNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Agent not found."));
        }

        if (result.Status == AddAgentToCompanyStatus.AlreadyAssociated)
        {
            return BadRequest(ApiResponse<object>.Fail("Agent is already associated with this photography company."));
        }

        return StatusCode(201, ApiResponse<AddAgentToPhotographyCompanyResponse>.Ok(result.Data!, "Agent added to photography company successfully."));
    }

    [HttpPost("agents/create")]
    public async Task<IActionResult> CreateAgent(CreateAgentRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity."));
        }
        CreateAgentResult result = await _photographyCompanyService.CreateAgentAsync(userId, request);

        if(result.Status == CreateAgentStatus.CompanyNotFound)
        {
            return NotFound(ApiResponse<object>.Fail("Photography company not found."));
        }

        if(result.Status == CreateAgentStatus.EmailAlreadyExists)
        {
            return Conflict(ApiResponse<object>.Fail("Email is already registered."));
        }

        if(result.Status == CreateAgentStatus.UserCreationFailed)
        {
            return StatusCode(500, ApiResponse<object>.Fail("Failed to create agent account."));
        }

        if(result.Status == CreateAgentStatus.RoleAssignmentFailed)
        {
            return StatusCode(500, ApiResponse<object>.Fail("Failed to assign agent role."));
        }

        if(result.Status == CreateAgentStatus.CompanyAssociationFailed)
        {
            return StatusCode(500, ApiResponse<object>.Fail("Failed to associate agent with photography company."));
        }

        if(result.Status == CreateAgentStatus.EmailSendingFailed)
        {
            ApiResponse<CreateAgentResponse> response = new ApiResponse<CreateAgentResponse>()
            {
                Success	= false,
                Message	= "Agent account created, but the login email could not be sent. Do not recreate the account.",
                Data = result.Data,
            };

            return StatusCode(201, response);
        }

        if(result.Status == CreateAgentStatus.Success)
        {
            return StatusCode(201, ApiResponse<CreateAgentResponse>.Ok(result.Data!,"Agent created and login email sent successfully."));
        }

        return StatusCode(500, ApiResponse<object>.Fail("Unexpected create-agent result."));
    }

}