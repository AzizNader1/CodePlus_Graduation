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
                return ApiResponse<T>.Failure($"Empty response from server (HTTP {(int)response.StatusCode})", statusCode: (int)response.StatusCode);
            }

            return JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing GET request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"API Connection Error: {ex.Message}", statusCode: 500);
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
                return ApiResponse<T>.Failure($"Empty response from server (HTTP {(int)response.StatusCode})", statusCode: (int)response.StatusCode);
            }

            return JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing POST request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"API Connection Error: {ex.Message}", statusCode: 500);
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
                return ApiResponse.Failure($"Empty response from server (HTTP {(int)response.StatusCode})", statusCode: (int)response.StatusCode);
            }

            return JsonSerializer.Deserialize<ApiResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing POST command to {Endpoint}", endpoint);
            return ApiResponse.Failure($"API Connection Error: {ex.Message}", statusCode: 500);
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
                return ApiResponse<T>.Failure($"Empty response from server (HTTP {(int)response.StatusCode})", statusCode: (int)response.StatusCode);
            }

            return JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing PUT request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"API Connection Error: {ex.Message}", statusCode: 500);
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
                return ApiResponse.Failure($"Empty response from server (HTTP {(int)response.StatusCode})", statusCode: (int)response.StatusCode);
            }

            return JsonSerializer.Deserialize<ApiResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing PUT command to {Endpoint}", endpoint);
            return ApiResponse.Failure($"API Connection Error: {ex.Message}", statusCode: 500);
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
                return ApiResponse<T>.Failure($"Empty response from server (HTTP {(int)response.StatusCode})", statusCode: (int)response.StatusCode);
            }

            return JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing DELETE request to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"API Connection Error: {ex.Message}", statusCode: 500);
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
                return ApiResponse.Failure($"Empty response from server (HTTP {(int)response.StatusCode})", statusCode: (int)response.StatusCode);
            }

            return JsonSerializer.Deserialize<ApiResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing DELETE command to {Endpoint}", endpoint);
            return ApiResponse.Failure($"API Connection Error: {ex.Message}", statusCode: 500);
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

            return JsonSerializer.Deserialize<ApiResponse<T>>(responseBody, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing multipart POST to {Endpoint}", endpoint);
            return ApiResponse<T>.Failure($"API Connection Error: {ex.Message}", statusCode: 500);
        }
    }
}
