using Remp.Models.Enums;

namespace Remp.Models.Responses;

public class ListingCaseMediaResponse
{
    public int Id {get; set;}
    public MediaType MediaType {get; set;}
    public string MediaUrl {get; set;} = "";
    public DateTime UploadedAt {get; set;}
    public bool IsSelect {get; set;}
    public bool IsHero {get; set;}
}