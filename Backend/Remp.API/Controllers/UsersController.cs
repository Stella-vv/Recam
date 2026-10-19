using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.DataAccess.Data;
using Remp.Models.Requests;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Responses;
using System.Security.Claims;

namespace Remp.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public UsersController(ApplicationDbContext context){
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "photographyCompany")]
    public async Task<IActionResult> GetUsers ([FromQuery] GetUsersRequest request)
    {
        var query = _context.Users.Where(user => !user.IsDeleted);
        int totalCount = await query.CountAsync();
        var orderedQuery = query.OrderBy(user => user.Id);
        long skipCount = (long)(request.PageNumber - 1) * request.PageSize;
        List<UserResponse> users = new List<UserResponse>();
        if ( skipCount < totalCount)
        {
            var pageQuery = orderedQuery.Skip((int)skipCount).Take(request.PageSize);
            var responseQuery = pageQuery.Select( user => new UserResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                AgentFirstName = user.Agent != null ? user.Agent.AgentFirstName : string.Empty,
                AgentLastName = user.Agent != null ? user.Agent.AgentLastName : string.Empty,
                CompanyName = user.Agent != null ? user.Agent.CompanyName : null
            } );
            users = await responseQuery.ToListAsync();
        }

        GetUsersResponse response = new GetUsersResponse()
        {
            Users = users,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize

        };
        return Ok(ApiResponse<GetUsersResponse>.Ok(response));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<GetCurrentUserResponse>.Fail("Invalid user identity."));
        }

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync( user => user.Id == userId && user.IsDeleted == false);
        if(user == null)
        {
            return Unauthorized(ApiResponse<GetCurrentUserResponse>.Fail("User not found or deleted."));
        }

        List<string> roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToList();

        List<int> assignedListingIds = await _context.ListingCases.AsNoTracking().Where( listingCase => listingCase.IsDeleted == false
                                                                                 && listingCase.AgentListingCases.Any(assignment => assignment.AgentId == userId))
                                                                                 .OrderBy(listingCase => listingCase.Id).Select(listingCase => listingCase.Id).ToListAsync();

        GetCurrentUserResponse response = new GetCurrentUserResponse()
        {
            UserId = userId,
            Roles = roles,
            AssignedListingIds = assignedListingIds
        };

        return Ok(ApiResponse<GetCurrentUserResponse>.Ok(response));

    }
}