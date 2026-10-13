using Remp.Models.Enums;

namespace Remp.Models.Responses;

public class CreateListingCaseResponse
{
    public int Id {get; set;}
    public ListingCaseStatus ListingCaseStatus {get; set;}
    public DateTime CreatedAt {get; set;}
}