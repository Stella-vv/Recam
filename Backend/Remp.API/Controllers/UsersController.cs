using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.DataAccess.Data;
using Remp.Models.Requests;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Responses;
using System.Security.Claims;
using Remp.API.Services.Users;

namespace Remp.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;
    public UsersController(ApplicationDbContext context, IUserService userService){
        _context = context;
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "photographyCompany")]
    public async Task<IActionResult> GetUsers ([FromQuery] GetUsersRequest request)
    {
        GetUsersResponse response = await _userService.GetUsersAsync(request);
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