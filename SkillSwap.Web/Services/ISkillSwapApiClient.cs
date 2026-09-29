using Microsoft.AspNetCore.Http;
using SkillSwap.Application.Common.Models;

namespace SkillSwap.Web.Services;

public interface ISkillSwapApiClient
{
    Task<ApiResponse<T>?> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object? data = null);
    Task<ApiResponse?> PostCommandAsync(string endpoint, object? data = null);
    Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object? data = null);
    Task<ApiResponse?> PutCommandAsync(string endpoint, object? data = null);
    Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint);
    Task<ApiResponse?> DeleteCommandAsync(string endpoint);
    Task<ApiResponse<T>?> PostMultipartAsync<T>(string endpoint, IFormFile file, string fieldName = "file");
}
