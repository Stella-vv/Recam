using Remp.Models.Responses;

namespace Remp.API.Middlewares;

public class ExceptionHandlingMiddleware{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        
    }

    public async Task InvokeAsync(HttpContext context)
    {
      try
      {
        await _next(context);
      }
      catch(Exception ex)
      {
          _logger.LogError(ex, "An unhandled exception occurred.");
          if (context.Response.HasStarted == true)
          {
            throw;
          }

          context.Response.Clear();
          context.Response.StatusCode = 500;
          ApiResponse<string> response = ApiResponse<string>.Fail("An unexpected error occurred. Please try again later.");
          await context.Response.WriteAsJsonAsync(response);
      }

    }
  
}