using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Remp.Models.Requests;
using Remp.Models.Responses;
using System.Security.Claims;
using Remp.API.Services.Users;
using Remp.Models.Results;
using Remp.Models.Enums;

namespace Remp.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService){
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

        List<string> roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToList();

        GetCurrentUserResponse? response = await _userService.GetCurrentUserAsync(userId, roles);

        if(response == null)
        {
            return Unauthorized(ApiResponse<GetCurrentUserResponse>.Fail("User not found or deleted."));
        }

        return Ok(ApiResponse<GetCurrentUserResponse>.Ok(response));

    }

    [HttpPut("me/password")]
    public async Task<IActionResult> UpdateAccountPassword([FromBody] UpdateAccountPasswordRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity."));
        }

        UpdateAccountPasswordResult result = await _userService.UpdateAccountPasswordAsync(userId, request);
        if(result.Status == UpdateAccountPasswordStatus.Success)
        {
            return NoContent();
        }

        if(result.Status == UpdateAccountPasswordStatus.UserNotFound)
        {
            return Unauthorized(ApiResponse<object>.Fail("User not found or deleted."));
        }

        if(result.Status == UpdateAccountPasswordStatus.InvalidCurrentPassword)
        {
            return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "Current password is incorrect."));
        }

        if(result.Status == UpdateAccountPasswordStatus.InvalidNewPassword)
        {
            return BadRequest(ApiResponse<object>.Fail(result.ErrorMessage ?? "New password does not meet password requirements."));
        }

        return StatusCode(500, "Unable to update password. Please try again.");
    }
}