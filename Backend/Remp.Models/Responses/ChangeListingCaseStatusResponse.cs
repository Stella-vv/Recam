using Remp.Models.Enums;

namespace Remp.Models.Responses;


public class ChangeListingCaseStatusResponse
{
    public int Id {get; set;}
    public ListingCaseStatus ListingCaseStatus {get; set;}
}