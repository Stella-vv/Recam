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
 
}