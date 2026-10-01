using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using SkillSwap.Application.Common.Models;

namespace SkillSwap.Web.Services;

public class SkillSwapApiClient : ISkillSwapApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SkillSwapApiClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private const string FriendlyConnectionErrorMessage = "We couldn't connect to the SkillSwap service right now. Please check your internet connection or try again in a moment.";
    private const string FriendlyServerIssueMessage = "We encountered a temporary issue while processing your request. Please try again.";

    public SkillSwapApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<SkillSwapApiClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private void AttachBearerToken(HttpRequestMessage request)
    {
        var context = _httpContextAccessor.HttpContext;
        var token = context?.User?.FindFirst("AccessToken")?.Value 
                    ?? context?.Session?.GetString("AccessToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string endpoint)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AttachBearerToken(request);

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse<T>.Failure(FriendlyServerIssueMessage, statusCode: (int)response.StatusCode);
            }

            return SanitizeResponse(JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing GET request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure(FriendlyConnectionErrorMessage, statusCode: 500);
        }
    }

    public async Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            AttachBearerToken(request);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse<T>.Failure(FriendlyServerIssueMessage, statusCode: (int)response.StatusCode);
            }

            return SanitizeResponse(JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing POST request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure(FriendlyConnectionErrorMessage, statusCode: 500);
        }
    }

    public async Task<ApiResponse?> PostCommandAsync(string endpoint, object? data = null)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            AttachBearerToken(request);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse.Failure(FriendlyServerIssueMessage, statusCode: (int)response.StatusCode);
            }

            return SanitizeResponse(JsonSerializer.Deserialize<ApiResponse>(content, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing POST command to {Endpoint}", endpoint);
            return ApiResponse.Failure(FriendlyConnectionErrorMessage, statusCode: 500);
        }
    }

    public async Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            AttachBearerToken(request);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse<T>.Failure(FriendlyServerIssueMessage, statusCode: (int)response.StatusCode);
            }

            return SanitizeResponse(JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing PUT request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure(FriendlyConnectionErrorMessage, statusCode: 500);
        }
    }

    public async Task<ApiResponse?> PutCommandAsync(string endpoint, object? data = null)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            AttachBearerToken(request);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, JsonOptions);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse.Failure(FriendlyServerIssueMessage, statusCode: (int)response.StatusCode);
            }

            return SanitizeResponse(JsonSerializer.Deserialize<ApiResponse>(content, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing PUT command to {Endpoint}", endpoint);
            return ApiResponse.Failure(FriendlyConnectionErrorMessage, statusCode: 500);
        }
    }

    public async Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            AttachBearerToken(request);

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse<T>.Failure(FriendlyServerIssueMessage, statusCode: (int)response.StatusCode);
            }

            return SanitizeResponse(JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing DELETE request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure(FriendlyConnectionErrorMessage, statusCode: 500);
        }
    }

    public async Task<ApiResponse?> DeleteCommandAsync(string endpoint)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            AttachBearerToken(request);

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse.Failure(FriendlyServerIssueMessage, statusCode: (int)response.StatusCode);
            }

            return SanitizeResponse(JsonSerializer.Deserialize<ApiResponse>(content, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing DELETE command to {Endpoint}", endpoint);
            return ApiResponse.Failure(FriendlyConnectionErrorMessage, statusCode: 500);
        }
    }

    public async Task<ApiResponse<T>?> PostMultipartAsync<T>(string endpoint, IFormFile file, string fieldName = "file")
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            AttachBearerToken(request);

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, fieldName, file.FileName);

            request.Content = content;

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            return SanitizeResponse(JsonSerializer.Deserialize<ApiResponse<T>>(responseBody, JsonOptions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing multipart POST to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure(FriendlyConnectionErrorMessage, statusCode: 500);
        }
    }

    private static ApiResponse<T> SanitizeResponse<T>(ApiResponse<T>? response)
    {
        if (response == null)
        {
            return ApiResponse<T>.Failure(FriendlyServerIssueMessage);
        }

        if (!response.IsSuccess && !string.IsNullOrWhiteSpace(response.Message))
        {
            response.Message = HumanizeMessage(response.Message);
        }

        return response;
    }

    private static ApiResponse SanitizeResponse(ApiResponse? response)
    {
        if (response == null)
        {
            return ApiResponse.Failure(FriendlyServerIssueMessage);
        }

        if (!response.IsSuccess && !string.IsNullOrWhiteSpace(response.Message))
        {
            response.Message = HumanizeMessage(response.Message);
        }

        return response;
    }

    private static string HumanizeMessage(string message)
    {
        var trimmed = message.Trim();

        if (string.Equals(trimmed, "Unauthorized", StringComparison.OrdinalIgnoreCase))
            return "Please sign in to access this feature.";

        if (string.Equals(trimmed, "Forbidden", StringComparison.OrdinalIgnoreCase))
            return "You don't have permission to perform this action.";

        if (trimmed.EndsWith("not found.", StringComparison.OrdinalIgnoreCase) ||
            trimmed.EndsWith("not found", StringComparison.OrdinalIgnoreCase))
        {
            if (trimmed.Contains("Session", StringComparison.OrdinalIgnoreCase))
                return "We couldn't locate this session. It may have been completed or removed.";
            if (trimmed.Contains("Swap request", StringComparison.OrdinalIgnoreCase))
                return "We couldn't find this swap request. It may have been updated or removed.";
            if (trimmed.Contains("User", StringComparison.OrdinalIgnoreCase))
                return "We couldn't find the requested member profile.";
            if (trimmed.Contains("Conversation", StringComparison.OrdinalIgnoreCase))
                return "We couldn't find this chat conversation.";
            return trimmed;
        }

        if (trimmed.Contains("Exception", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("NullReference", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("Object reference", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("Sequence contains no elements", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("An error occurred while processing your request", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("Internal Server Error", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("SqlException", StringComparison.OrdinalIgnoreCase))
        {
            return FriendlyServerIssueMessage;
        }

        return message;
    }
}
