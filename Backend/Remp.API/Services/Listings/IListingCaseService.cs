using Remp.Models.Requests;
using Remp.Models.Responses;
using Remp.Models.Results;

namespace Remp.API.Services.Listings;

public interface IListingCaseService
{
    public Task<CreateListingCaseResponse> CreateAsync(CreateListingCaseRequest request, string userId);
    public Task<GetListingCasesResponse?> GetListingCasesAsync(GetListingCasesRequest request, string userId, List<string> roles);
    public Task<UpdateListingCaseResponse?> UpdateAsync(UpdateListingCaseRequest request, int id, string userId);
    public Task<bool> DeleteAsync(int id, string userId);
    public Task<ChangeListingCaseStatusResult> ChangeStatusAsync(ChangeListingCaseStatusRequest request, int id, string userId);
    public Task<GetListingCaseDetailsResult> GetListingCaseDetailsAsync(int id, string userId, List<string> roles);
    public Task<GetListingCaseMediaResult> GetListingCaseMediaAsync(int id, string userId, List<string> roles);
    public Task<GetListingCaseContactsResult> GetListingCaseContactsAsync(int id, string userId, List<string> roles);
}