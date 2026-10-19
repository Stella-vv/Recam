namespace Remp.Models.Responses;

public class GetListingCaseMediaResponse
{
    public List<ListingCaseMediaResponse> Photos {get; set;} = [];
    public List<ListingCaseMediaResponse> Videos {get; set;} = [];
    public List<ListingCaseMediaResponse> FloorPlans {get; set;} = [];
    public List<ListingCaseMediaResponse> VrTours {get; set;} = [];
}