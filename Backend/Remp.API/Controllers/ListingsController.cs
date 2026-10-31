using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Remp.Models.Requests;
using Remp.Models.Responses;
using System.Security.Claims;
using Remp.Models.Enums;
using Remp.API.Services.Listings;
using Remp.Models.Results;

namespace Remp.API.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly IListingCaseService _listingCaseService;

    public ListingsController(IListingCaseService listingCaseService)
    {
        _listingCaseService = listingCaseService;
    }

    [Authorize(Roles = "photographyCompany")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateListingCaseRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<CreateListingCaseResponse>.Fail("Invalid user identity."));
        }

        CreateListingCaseResponse response = await _listingCaseService.CreateAsync(request, userId);

        return StatusCode(201, ApiResponse<CreateListingCaseResponse>.Ok(response,"Listing case created successfully."));
    }

    [HttpGet]
    public async Task<IActionResult> GetListingCases([FromQuery] GetListingCasesRequest request)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<GetListingCasesResponse>.Fail("Invalid user identity."));
        }

        List<string> roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToList();
        GetListingCasesResponse? response = await _listingCaseService.GetListingCasesAsync(request, userId, roles);
        if(response == null)
        {
            return Forbid();
        }
        else
        {
            return Ok(ApiResponse<GetListingCasesResponse>.Ok(response));
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "photographyCompany")]
    public async Task<IActionResult> Update([FromBody] UpdateListingCaseRequest request, int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<UpdateListingCaseResponse>.Fail("Invalid user identity."));
        }

        UpdateListingCaseResponse? response = await _listingCaseService.UpdateAsync(request, id, userId);
        if(response == null)
        {
            return NotFound(ApiResponse<UpdateListingCaseResponse>.Fail("Listing case not found."));
        }

        return Ok(ApiResponse<UpdateListingCaseResponse>.Ok(response, "Listing case updated successfully."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "photographyCompany")]

    public async Task<IActionResult> Delete(int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<object>.Fail("Invalid user identity."));
        }

        bool deleted = await _listingCaseService.DeleteAsync(id, userId);
        if(deleted == false)
        {
            return NotFound(ApiResponse<object>.Fail("Listing case not found."));
        }

        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetListingCaseDetails(int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<GetListingCaseDetailsResponse>.Fail("Invalid user identity."));
        }

        List<string> roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToList();
        GetListingCaseDetailsResult result = await _listingCaseService.GetListingCaseDetailsAsync(id, userId, roles);

        if(result.Status == GetListingCaseDetailsStatus.Forbidden)
        {
            return Forbid();
        }
        else if(result.Status == GetListingCaseDetailsStatus.ListingCaseNotFound)
        {
            return NotFound(ApiResponse<GetListingCaseDetailsResponse>.Fail("Listing case not found."));
        }

        return Ok(ApiResponse<GetListingCaseDetailsResponse>.Ok(result.Data!));
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "photographyCompany")]
    public async Task<IActionResult> ChangeStatus([FromBody] ChangeListingCaseStatusRequest request, int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<ChangeListingCaseStatusResponse>.Fail("Invalid user identity."));
        }

        ChangeListingCaseStatusResult result = await _listingCaseService.ChangeStatusAsync(request, id, userId);
        if(result.Status == ChangeListingCaseStatusResultStatus.ListingCaseNotFound)
        {
            return NotFound(ApiResponse<ChangeListingCaseStatusResponse>.Fail("Listing case not found."));
        }

        if(result.Status == ChangeListingCaseStatusResultStatus.InvalidTransition)
        {
            return BadRequest(ApiResponse<ChangeListingCaseStatusResponse>.Fail("Invalid listing case status transition."));
        }

        return Ok(ApiResponse<ChangeListingCaseStatusResponse>.Ok(result.Data!, "Listing case status changed successfully."));
    }

    [HttpGet("{id:int}/media")]
    public async Task<IActionResult> GetListingCaseMedia(int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<GetListingCaseMediaResponse>.Fail("Invalid user identity."));
        }

        List<string> roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToList();
        GetListingCaseMediaResult result = await _listingCaseService.GetListingCaseMediaAsync(id, userId, roles);

        if(result.Status == GetListingCaseMediaStatus.ListingCaseNotFound)
        {
            return NotFound(ApiResponse<GetListingCaseMediaResponse>.Fail("Listing case not found."));
        }

        if(result.Status == GetListingCaseMediaStatus.Forbidden)
        {
            return Forbid();
        }

        return Ok(ApiResponse<GetListingCaseMediaResponse>.Ok(result.Data!));
    }

    [HttpGet("{id:int}/contacts")]
    public async Task<IActionResult> GetListingCaseContacts(int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<GetListingCaseContactsResponse>.Fail("Invalid user identity."));
        }

        List<string> roles = User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).Distinct().ToList();

        GetListingCaseContactsResult result = await _listingCaseService.GetListingCaseContactsAsync(id, userId, roles);

        if(result.Status == GetListingCaseContactsStatus.ListingCaseNotFound)
        {
            return NotFound(ApiResponse<GetListingCaseContactsResponse>.Fail("Listing case not found."));
        }

        if(result.Status == GetListingCaseContactsStatus.Forbidden)
        {
            return Forbid();
        }

        return Ok(ApiResponse<GetListingCaseContactsResponse>.Ok(result.Data!));
    }
}