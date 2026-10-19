namespace Remp.Models.Responses;

public class GetCurrentUserResponse
{
    public string UserId {get; set;} = string.Empty;
    public List<string> Roles {get; set;} = new List<string>();
    public List<int> AssignedListingIds {get; set;} = new List<int>();
}