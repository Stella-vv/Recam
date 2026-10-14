using Remp.Models.Enums;
namespace Remp.Models.Responses;

public class ListingCaseResponse
{
    public int Id {get; set; } 
    public string Street {get; set; } = string.Empty;
    public string City {get; set; } = string.Empty;
    public string State {get; set; } = string.Empty;
    public int Postcode {get; set; }
    public ListingCaseStatus ListingCaseStatus {get; set; }
    public DateTime CreatedAt {get; set; }
}