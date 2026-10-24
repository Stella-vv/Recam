using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.DataAccess.Data;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Remp.Models.Entities;
using Microsoft.Data.SqlClient;

namespace Remp.API.Controllers;


[ApiController]
[Route("api/photography-companies")]
[Authorize(Roles = "photographyCompany")]
public class PhotographyCompaniesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PhotographyCompaniesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("agents")]
    public async Task<IActionResult> AddAgent(AddAgentToPhotographyCompanyRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity."));
        }

        var company = await _context.PhotographyCompanies.AsNoTracking().FirstOrDefaultAsync(company => company.Id == userId);
        if(company == null)
        {
            return NotFound(ApiResponse<object>.Fail("Photography company not found."));
        }

        var agent = await _context.Agents.AsNoTracking().FirstOrDefaultAsync( agent => agent.Id == request.AgentId && agent.User != null && agent.User.IsDeleted == false);
        if(agent == null)
        {
            return NotFound(ApiResponse<object>.Fail("Agent not found."));
        }

        var existingRelation = await _context.AgentPhotographyCompanies.FirstOrDefaultAsync(relation => relation.AgentId == request.AgentId && relation.PhotographyCompanyId == userId);
        if(existingRelation != null)
        {
            return BadRequest(ApiResponse<object>.Fail("Agent is already associated with this photography company."));
        }

        AgentPhotographyCompany relation = new AgentPhotographyCompany()
        {
            AgentId	= request.AgentId,
            PhotographyCompanyId = userId
        };

        _context.AgentPhotographyCompanies.Add(relation);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
        {
            return BadRequest(ApiResponse<object>.Fail("Agent is already associated with this photography company."));
        }

        AddAgentToPhotographyCompanyResponse response = new AddAgentToPhotographyCompanyResponse()
        {
            AgentId = relation.AgentId,
            PhotographyCompanyId = relation.PhotographyCompanyId
        };

        return StatusCode(201, ApiResponse<AddAgentToPhotographyCompanyResponse>.Ok(response, "Agent added to photography company successfully."));
    }

}