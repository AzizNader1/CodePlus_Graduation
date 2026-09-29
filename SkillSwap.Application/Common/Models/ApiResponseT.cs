namespace SkillSwap.Application.Common.Models;

/// <summary>
/// Universal generic API response envelope providing structured status code, expressiveness message, payload data, and errors.
/// </summary>
public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public string[] Errors { get; set; } = Array.Empty<string>();

    public static ApiResponse<T> Success(T? data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            IsSuccess = true,
            Message = message,
            Data = data,
            Errors = Array.Empty<string>()
        };
    }

    public static ApiResponse<T> Failure(string message, IEnumerable<string>? errors = null, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            IsSuccess = false,
            Message = message,
            Data = default,
            Errors = errors?.ToArray() ?? (message != null ? new[] { message } : Array.Empty<string>())
        };
    }
}
