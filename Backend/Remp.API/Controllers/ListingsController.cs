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

namespace Remp.API.Controllers;

[ApiController]
[Route("api/listings")]
[Authorize(Roles = "photographyCompany")]
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

}