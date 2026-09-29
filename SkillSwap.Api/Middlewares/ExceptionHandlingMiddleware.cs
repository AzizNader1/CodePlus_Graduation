using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Exceptions;
using SkillSwap.Application.Common.Models;

namespace SkillSwap.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string message;
        string[]? errors = null;

        switch (exception)
        {
            case ValidationException valEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = valEx.Message ?? "One or more validation errors occurred.";
                errors = valEx.Errors.SelectMany(e => e.Value).ToArray();
                break;

            case NotFoundException nfEx:
                statusCode = (int)HttpStatusCode.NotFound;
                message = nfEx.Message;
                break;

            case ForbiddenAccessException faEx:
                statusCode = (int)HttpStatusCode.Forbidden;
                message = faEx.Message;
                break;

            case BadRequestException brEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = brEx.Message;
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred on the server.";
                break;
        }

        context.Response.StatusCode = statusCode;
        var apiResponse = ApiResponse.Failure(message, errors, statusCode);
        var json = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(json);
    }
}
