using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Remp.DataAccess.Data;
using Remp.DataAccess.Repositories;
using Remp.Models.Requests;
using Remp.Models.Responses;
using System.Security.Claims;
using Microsoft.Identity.Client;
using Remp.Models.Entities;
using Remp.Models.Enums;
using Remp.Models.Logs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Remp.API.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize]
public class ListingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICaseHistoryRepository _caseHistoryRepository;
    private readonly ILogger<ListingsController> _logger;

    public ListingsController(ApplicationDbContext context, ICaseHistoryRepository caseHistoryRepository, ILogger<ListingsController> logger)
    {
        _context = context;
        _caseHistoryRepository = caseHistoryRepository;
        _logger = logger;
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

        ListingCase listingCase = new ListingCase()
        {
            Street = request.Street.Trim(),
            City = request.City.Trim(),
            State = request.State.Trim(),
            Postcode = request.Postcode,
            Bedrooms = request.Bedrooms,
            Bathrooms= request.Bathrooms,
            Garages= request.Garages,
            FloorArea = request.FloorArea,
            PropertyType = request.PropertyType,
            SaleCategory = request.SaleCategory,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ListingCaseStatus = ListingCaseStatus.Created,
            IsDeleted = false
        };

        _context.ListingCases.Add(listingCase);
        await _context.SaveChangesAsync();

        CaseHistoryLog log = new CaseHistoryLog()
        {
            ListingCaseId = listingCase.Id,
            UserId = userId,
            Action = "Created",
            CreatedAt = listingCase.CreatedAt
        };

        try
        {
            await _caseHistoryRepository.AddAsync(log);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex,"Failed to save case creation history for listing {ListingCaseId}.", listingCase.Id);
        }

        CreateListingCaseResponse response = new CreateListingCaseResponse()
        {
            Id = listingCase.Id,
            ListingCaseStatus = listingCase.ListingCaseStatus,
            CreatedAt = listingCase.CreatedAt
        };

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
        var query = _context.ListingCases.AsNoTracking().Where( listingCase => !listingCase.IsDeleted);
        if (User.IsInRole("photographyCompany"))
        {
            query = query.Where(listingCase => listingCase.UserId == userId);
        }
        else if (User.IsInRole("user"))
        {
            query = query.Where(listingCase => listingCase.AgentListingCases.Any(assignment => assignment.AgentId == userId));
        }
        else
        {
            return Forbid();
        }
        int totalCount = await query.CountAsync();
        var orderedQuery = query.OrderByDescending(listingCase => listingCase.CreatedAt).ThenByDescending(listingCase => listingCase.Id);

        long skipCount = (long)(request.PageNumber - 1) * request.PageSize;
        List<ListingCaseResponse> listingCases = new List<ListingCaseResponse>();
        if(skipCount < totalCount)
        {
            var pageQuery = orderedQuery.Skip((int)skipCount).Take(request.PageSize);
            var responseQuery = pageQuery.Select(listingCase => new ListingCaseResponse()
            {
                Id = listingCase.Id,
                Street = listingCase.Street,
                City = listingCase.City,
                State = listingCase.State,
                Postcode = listingCase.Postcode,
                ListingCaseStatus = listingCase.ListingCaseStatus,
                CreatedAt = listingCase.CreatedAt
            });
            listingCases = await responseQuery.ToListAsync();
        }

        GetListingCasesResponse response = new GetListingCasesResponse()
        {
            ListingCases = listingCases,
            TotalCount = totalCount,
            PageNumber= request.PageNumber,
            PageSize = request.PageSize
        };
        return Ok(ApiResponse<GetListingCasesResponse>.Ok(response));
    
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

        ListingCase? listingCase = await _context.ListingCases.FirstOrDefaultAsync(listingCase => (listingCase.Id == id && listingCase.IsDeleted == false && listingCase.UserId == userId));
        if(listingCase == null)
        {
            return NotFound(ApiResponse<UpdateListingCaseResponse>.Fail("Listing case not found."));
        }

        listingCase.Street = request.Street.Trim();
        listingCase.City = request.City.Trim();
        listingCase.State = request.State.Trim();
        listingCase.Postcode= request.Postcode;
        listingCase.Bedrooms = request.Bedrooms;
        listingCase.Bathrooms = request.Bathrooms;
        listingCase.Garages = request.Garages;
        listingCase.FloorArea = request.FloorArea;
        listingCase.PropertyType = request.PropertyType;
        listingCase.SaleCategory = request.SaleCategory;
        await _context.SaveChangesAsync();

        CaseHistoryLog log = new CaseHistoryLog()
        {
            ListingCaseId = listingCase.Id,
            UserId = userId,
            Action = "Updated",
            CreatedAt = DateTime.UtcNow
        };

        try{
            await _caseHistoryRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save case update history for listing {ListingCaseId}.", listingCase.Id);
        }

        UpdateListingCaseResponse response = new UpdateListingCaseResponse()
        {
            Id = listingCase.Id
        };
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

        ListingCase? listingCase = await _context.ListingCases.FirstOrDefaultAsync( listingCase => (listingCase.Id == id && listingCase.IsDeleted == false && listingCase.UserId == userId));
        if(listingCase == null)
        {
            return NotFound(ApiResponse<object>.Fail("Listing case not found."));
        }

        listingCase.IsDeleted = true;
        List<MediaAsset> mediaAssets = await _context.MediaAssets.Where(mediaAssets =>(mediaAssets.ListingCaseId == id && mediaAssets.IsDeleted == false)).ToListAsync();
        foreach(MediaAsset mediaAsset in mediaAssets)
        {
            mediaAsset.IsDeleted = true;
        }
        await _context.SaveChangesAsync();

        CaseHistoryLog log = new CaseHistoryLog()
        {
            ListingCaseId = listingCase.Id,
            UserId = userId,
            Action = "Deleted",
            CreatedAt = DateTime.UtcNow
        };

        try{
            await _caseHistoryRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save case deletion history for listing {ListingCaseId}.", listingCase.Id);
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

        var query = _context.ListingCases.AsNoTracking().Where( q => q.Id == id && q.IsDeleted == false);
        if (User.IsInRole("photographyCompany"))
        {
            query = query.Where( q => q.UserId == userId);
        }
        else if (User.IsInRole("user"))
        {
            query = query.Where(q => q.AgentListingCases.Any(assignment => assignment.AgentId == userId));
        }
        else
        {
            return Forbid();
        }

        ListingCase? listingCase = await query.Include(q => q.MediaAssets.Where( m => m.IsDeleted == false))
                                              .Include(q => q.AgentListingCases)
                                              .ThenInclude(q => q.Agent)
                                              .ThenInclude(q => q!.User)
                                              .FirstOrDefaultAsync();
        if(listingCase == null)
        {
            return NotFound(ApiResponse<GetListingCaseDetailsResponse>.Fail("Listing case not found."));
        }

        GetListingCaseDetailsResponse response = new GetListingCaseDetailsResponse()
        {
            Id = listingCase.Id,
            Title = listingCase.Title,
            Description = listingCase.Description,
            Street = listingCase.Street,
            City = listingCase.City,
            State = listingCase.State,
            Postcode = listingCase.Postcode,
            Bedrooms = listingCase.Bedrooms,
            Bathrooms = listingCase.Bathrooms,
            Garages = listingCase.Garages,
            FloorArea = listingCase.FloorArea,
            Price = listingCase.Price,
            Longitude = listingCase.Longitude,
            Latitude = listingCase.Latitude,
            CreatedAt = listingCase.CreatedAt,
            PropertyType = listingCase.PropertyType,
            SaleCategory = listingCase.SaleCategory,
            ListingCaseStatus = listingCase.ListingCaseStatus
        };

        response.MediaAssets = listingCase.MediaAssets.OrderBy(media => media.Id).Select(media => new ListingCaseMediaResponse()
                                                                                 {
                                                                                        Id = media.Id,
                                                                                        MediaType = media.MediaType,
                                                                                        MediaUrl = media.MediaUrl,
                                                                                        UploadedAt = media.UploadedAt,
                                                                                        IsSelect = media.IsSelect,
                                                                                        IsHero = media.IsHero
                                                                                 }).ToList();
        response.Agents = listingCase.AgentListingCases.Select(assignment => assignment.Agent)
                                                       .Where(agent => agent != null && agent.User != null && agent.User.IsDeleted == false).OrderBy(agent => agent!.Id)
                                                       .Select(agent => new ListingCaseAgentResponse()
                                                       {
                                                            Id = agent!.Id,
                                                            AgentFirstName = agent!.AgentFirstName,
                                                            AgentLastName = agent!.AgentLastName,
                                                            AvatarUrl = agent!.AvatarUrl,
                                                            CompanyName = agent!.CompanyName,
                                                            Email = agent!.User!.Email,
                                                            PhoneNumber = agent!.User!.PhoneNumber
                                                       }).ToList();

        return Ok(ApiResponse<GetListingCaseDetailsResponse>.Ok(response));
    }

 
}