namespace Remp.Models.Responses;

public class ApiResponse<T>
{
    public bool Success {get; set;}
    public string Message {get; set;} = "";
    public T? Data {get; set;}

    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
      ApiResponse<T> response = new ApiResponse<T>
      {
        Success = true,
        Message = message,
        Data = data
      };

      return response;
    }

    public static ApiResponse<T> Fail(string message)
    {
        ApiResponse<T> response = new ApiResponse<T>
        {
            Success = false,
            Message = message

        };

        return response;
    }

}