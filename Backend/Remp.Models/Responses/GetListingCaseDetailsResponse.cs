using Remp.Models.Enums;

namespace Remp.Models.Responses;

public class GetListingCaseDetailsResponse
{
    public int	Id {get; set;}
    public string? Title {get; set;}
    public string? Description {get; set;}
    public string	Street {get; set;} = "";
    public string City {get; set;} = "";
    public string State {get; set;} = "";
    public int Postcode {get; set;}
    public int Bedrooms {get; set;}
    public int Bathrooms {get; set;}
    public int Garages {get; set;}
    public double	FloorArea {get; set;}
    public double	Price {get; set;}
    public decimal Longitude {get; set;}
    public decimal Latitude {get; set;}
    public DateTime	CreatedAt {get; set;}
    public PropertyType	PropertyType {get; set;}
    public SaleCategory	SaleCategory {get; set;}
    public ListingCaseStatus ListingCaseStatus {get; set;}
    public List<ListingCaseMediaResponse> MediaAssets {get; set;}	= new List<ListingCaseMediaResponse>();
    public List<ListingCaseAgentResponse> Agents {get; set;} = new List<ListingCaseAgentResponse>();

}