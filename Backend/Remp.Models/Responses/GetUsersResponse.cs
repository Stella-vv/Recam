namespace Remp.Models.Responses;

public class GetUsersResponse
{
    public List<UserResponse> Users { get; set; } = new List<UserResponse>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

}