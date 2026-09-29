namespace SkillSwap.Application.Common.Models;

/// <summary>
/// Universal non-generic API response envelope providing structured status code, expressiveness message, and errors.
/// </summary>
public class ApiResponse
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string[] Errors { get; set; } = Array.Empty<string>();

    public static ApiResponse Success(string message = "Operation completed successfully.", int statusCode = 200)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            IsSuccess = true,
            Message = message,
            Errors = Array.Empty<string>()
        };
    }

    public static ApiResponse Failure(string message, IEnumerable<string>? errors = null, int statusCode = 400)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            IsSuccess = false,
            Message = message,
            Errors = errors?.ToArray() ?? (message != null ? new[] { message } : Array.Empty<string>())
        };
    }
}
