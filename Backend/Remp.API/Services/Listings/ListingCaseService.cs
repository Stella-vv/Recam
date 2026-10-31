

using Remp.DataAccess.Repositories;
using Remp.Models.Entities;
using Remp.Models.Enums;
using Remp.Models.Logs;
using Remp.Models.Requests;
using Remp.Models.Responses;
using Remp.Models.Results;
using SharpCompress.Compressors.ZStandard.Unsafe;

namespace Remp.API.Services.Listings;

public class ListingCaseService : IListingCaseService
{
    private readonly IListingCaseRepository	_listingCaseRepository;
    private readonly ICaseHistoryRepository	_caseHistoryRepository;
    private readonly ILogger<ListingCaseService> _logger;
    private readonly IMediaRepository _mediaRepository;
    private readonly ICaseContactRepository _caseContactRepository;

    public ListingCaseService(IListingCaseRepository listingCaseRepository, ICaseHistoryRepository	caseHistoryRepository, ILogger<ListingCaseService> logger, IMediaRepository mediaRepository, ICaseContactRepository caseContactRepository)
    {
        _listingCaseRepository = listingCaseRepository;
        _caseHistoryRepository = caseHistoryRepository;
        _logger = logger;
        _mediaRepository = mediaRepository;
        _caseContactRepository = caseContactRepository;
    }

    public async Task<CreateListingCaseResponse> CreateAsync(CreateListingCaseRequest request, string userId)
    {
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

        await _listingCaseRepository.AddAsync(listingCase);

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
        return response;
    }

    public async Task<GetListingCasesResponse?> GetListingCasesAsync(GetListingCasesRequest request, string userId, List<string> roles)
    {
        bool isPhotographyCompany = roles.Contains("photographyCompany");
        if (!isPhotographyCompany && !roles.Contains("user"))
        {
            return null;
        }

        var result = await _listingCaseRepository.GetListingCasesAsync(userId, isPhotographyCompany, request.PageNumber, request.PageSize);

        var responseQuery = result.ListingCases.Select(listingCase => new ListingCaseResponse()
        {
            Id = listingCase.Id,
            Street = listingCase.Street,
            City = listingCase.City,
            State = listingCase.State,
            Postcode = listingCase.Postcode,
            ListingCaseStatus = listingCase.ListingCaseStatus,
            CreatedAt = listingCase.CreatedAt
        });

        List<ListingCaseResponse> listingCaseResponses = responseQuery.ToList();

        GetListingCasesResponse response = new GetListingCasesResponse()
        {
            ListingCases = listingCaseResponses,
            TotalCount = result.TotalCount,
            PageNumber= request.PageNumber,
            PageSize = request.PageSize
        };

        return response;
    }

    public async Task<UpdateListingCaseResponse?> UpdateAsync(UpdateListingCaseRequest request, int id, string userId)
    {
        ListingCase? listingCase = await _listingCaseRepository.GetOwnedListingCaseAsync(id, userId);
        if(listingCase == null)
        {
            return null;
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

        await _listingCaseRepository.SaveChangesAsync();

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

        return response;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        ListingCase? listingCase = await _listingCaseRepository.GetOwnedListingCaseAsync(id, userId);
        if (listingCase == null)
        {
            return false;
        }

        listingCase.IsDeleted = true;
        List<MediaAsset> mediaAssets = await _mediaRepository.GetByListingCaseIdAsync(id);
        foreach(MediaAsset m in mediaAssets)
        {
            m.IsDeleted = true;
        }
        await _listingCaseRepository.SaveChangesAsync();

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

        return true;
    }

    public async Task<ChangeListingCaseStatusResult> ChangeStatusAsync(ChangeListingCaseStatusRequest request, int id, string userId)
    {
        ListingCase? listingCase = await _listingCaseRepository.GetOwnedListingCaseAsync(id, userId);
        if(listingCase == null)
        {
            ChangeListingCaseStatusResult result = new ChangeListingCaseStatusResult()
            {
                Status = ChangeListingCaseStatusResultStatus.ListingCaseNotFound
            };
            return result;
        }

        ListingCaseStatus oldStatus = listingCase.ListingCaseStatus;
        ListingCaseStatus newStatus = request.ListingCaseStatus!.Value;
        bool isValidTransition = ((listingCase.ListingCaseStatus == ListingCaseStatus.Created && request.ListingCaseStatus == ListingCaseStatus.Pending)
                                 || (listingCase.ListingCaseStatus == ListingCaseStatus.Pending && request.ListingCaseStatus == ListingCaseStatus.InReview)
                                 || (listingCase.ListingCaseStatus == ListingCaseStatus.InReview && request.ListingCaseStatus == ListingCaseStatus.Delivered));
        if (!isValidTransition)
        {
            ChangeListingCaseStatusResult result = new ChangeListingCaseStatusResult()
            {
                Status = ChangeListingCaseStatusResultStatus.InvalidTransition
            };
            return result;
        }

        listingCase.ListingCaseStatus = newStatus;
        await _listingCaseRepository.SaveChangesAsync();

        CaseHistoryLog log = new CaseHistoryLog()
        {
            ListingCaseId = listingCase.Id,
            UserId = userId,
            Action = "StatusChanged",
            CreatedAt = DateTime.UtcNow,
            OldStatus = oldStatus,
            NewStatus = newStatus
        };

        try
        {
            await _caseHistoryRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save case status change history for listing {ListingCaseId}.", listingCase.Id);
        }

        ChangeListingCaseStatusResponse response = new ChangeListingCaseStatusResponse(){
            Id = listingCase.Id,
            ListingCaseStatus = listingCase.ListingCaseStatus
        };

        ChangeListingCaseStatusResult successResult = new ChangeListingCaseStatusResult()
        {
            Status = ChangeListingCaseStatusResultStatus.Success,
            Data = response
        };
        return successResult;
    }

    public async Task<GetListingCaseDetailsResult> GetListingCaseDetailsAsync(int id, string userId, List<string> roles)
    {
        bool isPhotographyCompany = roles.Contains("photographyCompany");
        if (!isPhotographyCompany && !roles.Contains("user"))
        {
            GetListingCaseDetailsResult result = new GetListingCaseDetailsResult()
            {
                Status = GetListingCaseDetailsStatus.Forbidden
            };
            return result;
        }
        ListingCase? listingCase = await _listingCaseRepository.GetAccessibleListingCaseDetailsAsync(id, userId, isPhotographyCompany);
        if(listingCase == null)
        {
            GetListingCaseDetailsResult result = new GetListingCaseDetailsResult()
            {
                Status = GetListingCaseDetailsStatus.ListingCaseNotFound
            };
            return result;
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

        response.MediaAssets = listingCase.MediaAssets.OrderBy(media => media.Id)
                                                      .Select(media => new ListingCaseMediaResponse
                                                      {
                                                          Id = media.Id,
                                                          MediaType = media.MediaType,
                                                          MediaUrl = media.MediaUrl,
                                                          UploadedAt = media.UploadedAt,
                                                          IsSelect = media.IsSelect,
                                                          IsHero = media.IsHero
                                                      }).ToList();

        response.Agents = listingCase.AgentListingCases.Select(assignment => assignment.Agent)
                                                       .Where(agent => agent != null && agent.User != null && agent.User.IsDeleted == false)
                                                       .OrderBy(agent => agent!.Id)
                                                       .Select(agent => new ListingCaseAgentResponse()
                                                       {
                                                           Id = agent!.Id,
                                                           AgentFirstName = agent.AgentFirstName,
                                                           AgentLastName = agent.AgentLastName,
                                                           AvatarUrl = agent.AvatarUrl,
                                                           CompanyName = agent.CompanyName,
                                                           Email = agent.User!.Email,
                                                           PhoneNumber = agent.User.PhoneNumber
                                                       }).ToList();

        GetListingCaseDetailsResult successResult = new GetListingCaseDetailsResult()
        {
            Status = GetListingCaseDetailsStatus.Success,
            Data = response
        };
        return successResult;
    }

    public async Task<GetListingCaseMediaResult> GetListingCaseMediaAsync(int id, string userId, List<string> roles)
    {
        bool isPhotographyCompany = roles.Contains("photographyCompany");
        if(!isPhotographyCompany && !roles.Contains("user"))
        {
            GetListingCaseMediaResult result = new GetListingCaseMediaResult()
            {
                Status = GetListingCaseMediaStatus.Forbidden
            };
            return result;
        }

        ListingCase? listingCase = await _listingCaseRepository.GetAccessibleListingCaseWithMediaAsync(id, userId, isPhotographyCompany);
        if(listingCase == null)
        {
            GetListingCaseMediaResult result = new GetListingCaseMediaResult()
            {
                Status = GetListingCaseMediaStatus.ListingCaseNotFound
            };
            return result;
        }

        List<ListingCaseMediaResponse> mediaResponses = listingCase.MediaAssets.OrderBy(media =>media.Id)
                                                                               .Select(media => new ListingCaseMediaResponse()
                                                                                             {
                                                                                                Id = media.Id,
                                                                                                MediaType = media.MediaType,
                                                                                                MediaUrl = media.MediaUrl,
                                                                                                UploadedAt = media.UploadedAt,
                                                                                                IsSelect = media.IsSelect,
                                                                                                IsHero = media.IsHero
                                                                                             }).ToList();

        GetListingCaseMediaResponse response = new GetListingCaseMediaResponse()
        {
            Photos = mediaResponses.Where(media => media.MediaType == MediaType.Photo).ToList(),
            Videos = mediaResponses.Where(media => media.MediaType == MediaType.Video).ToList(),
            FloorPlans = mediaResponses.Where(media => media.MediaType == MediaType.FloorPlan).ToList(),
            VrTours = mediaResponses.Where(media => media.MediaType == MediaType.VrTour).ToList()
        };

        GetListingCaseMediaResult successResult = new GetListingCaseMediaResult()
        {
            Status = GetListingCaseMediaStatus.Success,
            Data = response
        };
        return successResult;
    }

    public async Task<GetListingCaseContactsResult> GetListingCaseContactsAsync(int id, string userId, List<string> roles)
    {
        bool isPhotographyCompany = roles.Contains("photographyCompany");
        if(!isPhotographyCompany && !roles.Contains("user"))
        {
            GetListingCaseContactsResult result = new GetListingCaseContactsResult()
            {
                Status = GetListingCaseContactsStatus.Forbidden
            };
            return result;
        }

        bool canAccess = await _listingCaseRepository.CanAccessListingCaseAsync(id, userId, isPhotographyCompany);
        if (!canAccess)
        {
            GetListingCaseContactsResult result = new GetListingCaseContactsResult()
            {
                Status = GetListingCaseContactsStatus.ListingCaseNotFound
            };
            return result;
        }

        List<CaseContact> contacts = await _caseContactRepository.GetByListingCaseIdAsync(id);
        List<CaseContactResponse> contactResponses = contacts.Select(contact => new CaseContactResponse()
                                                                {
                                                                    ContactId = contact.ContactId,
                                                                    FirstName = contact.FirstName,
                                                                    LastName = contact.LastName,
                                                                    CompanyName = contact.CompanyName,
                                                                    ProfileUrl = contact.ProfileUrl,
                                                                    Email = contact.Email,
                                                                    PhoneNumber = contact.PhoneNumber
                                                                }).ToList();

        GetListingCaseContactsResponse response = new GetListingCaseContactsResponse(){
            Contacts = contactResponses
        };

        GetListingCaseContactsResult successResult = new GetListingCaseContactsResult()
        {
            Status = GetListingCaseContactsStatus.Success,
            Data = response
        };
        return successResult;
    }




}