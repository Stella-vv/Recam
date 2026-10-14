namespace Remp.Models.Responses;

public class GetListingCasesResponse
{
    public List<ListingCaseResponse> ListingCases {get; set; } = new List<ListingCaseResponse>();
    public int TotalCount {get; set;}
    public int PageNumber {get; set;}
    public int PageSize {get; set;}
}