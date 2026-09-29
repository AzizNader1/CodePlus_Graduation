using System.Collections;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SkillSwap.Application.Common.Models;

namespace SkillSwap.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected ActionResult HandleResult<T>(Result<T> result, string? customSuccessMessage = null)
    {
        var controller = GetControllerName();
        var action = GetActionName();

        if (result == null)
        {
            var notFoundMsg = ResolveEmptyMessage(controller, action);
            return StatusCode(StatusCodes.Status404NotFound, ApiResponse<T>.Failure(
                message: notFoundMsg,
                statusCode: StatusCodes.Status404NotFound));
        }

        if (result.IsSuccess)
        {
            // Case 1: Result value is null
            if (result.Value == null)
            {
                var emptyMsg = ResolveEmptyMessage(controller, action);
                return Ok(ApiResponse<T>.Success(
                    data: default,
                    message: emptyMsg,
                    statusCode: StatusCodes.Status200OK));
            }

            // Case 2: Result value is a collection/list
            if (result.Value is IEnumerable enumerable && !(result.Value is string))
            {
                var enumerator = enumerable.GetEnumerator();
                bool hasItems = enumerator.MoveNext();

                if (!hasItems)
                {
                    var emptyMsg = ResolveEmptyMessage(controller, action);
                    return Ok(ApiResponse<T>.Success(
                        data: result.Value,
                        message: emptyMsg,
                        statusCode: StatusCodes.Status200OK));
                }

                var successMsg = customSuccessMessage ?? ResolveSuccessMessage(controller, action);
                return Ok(ApiResponse<T>.Success(
                    data: result.Value,
                    message: successMsg,
                    statusCode: StatusCodes.Status200OK));
            }

            // Case 3: Result value contains an 'Items' collection (e.g. PaginatedList<T>)
            var valueType = result.Value.GetType();
            var itemsProp = valueType.GetProperty("Items");
            if (itemsProp != null && itemsProp.GetValue(result.Value) is IEnumerable itemsEnumerable && !(itemsProp.GetValue(result.Value) is string))
            {
                var itemsEnumerator = itemsEnumerable.GetEnumerator();
                bool hasItems = itemsEnumerator.MoveNext();

                if (!hasItems)
                {
                    var emptyMsg = ResolveEmptyMessage(controller, action);
                    return Ok(ApiResponse<T>.Success(
                        data: result.Value,
                        message: emptyMsg,
                        statusCode: StatusCodes.Status200OK));
                }
            }

            // Case 4: Result value is a boolean
            if (result.Value is bool boolVal)
            {
                var msg = customSuccessMessage ?? (boolVal ? ResolveSuccessMessage(controller, action) : "Operation could not be completed.");
                return Ok(ApiResponse<T>.Success(
                    data: result.Value,
                    message: msg,
                    statusCode: StatusCodes.Status200OK));
            }

            // Case 5: Standard entity / DTO payload
            var standardSuccessMsg = customSuccessMessage ?? ResolveSuccessMessage(controller, action);
            return Ok(ApiResponse<T>.Success(
                data: result.Value,
                message: standardSuccessMsg,
                statusCode: StatusCodes.Status200OK));
        }

        var errorMessage = !string.IsNullOrWhiteSpace(result.Error) ? result.Error : "One or more validation errors occurred.";
        int errorStatusCode = ResolveStatusCodeForError(errorMessage);

        return StatusCode(errorStatusCode, ApiResponse<T>.Failure(
            message: errorMessage,
            errors: result.Errors,
            statusCode: errorStatusCode));
    }

    protected ActionResult HandleResult(Result result, string? customSuccessMessage = null)
    {
        var controller = GetControllerName();
        var action = GetActionName();

        if (result == null)
        {
            return StatusCode(StatusCodes.Status404NotFound, ApiResponse.Failure(
                message: "Requested resource was not found.",
                statusCode: StatusCodes.Status404NotFound));
        }

        if (result.IsSuccess)
        {
            var successMsg = customSuccessMessage ?? ResolveSuccessMessage(controller, action);
            return Ok(ApiResponse.Success(
                message: successMsg,
                statusCode: StatusCodes.Status200OK));
        }

        var errorMessage = !string.IsNullOrWhiteSpace(result.Error) ? result.Error : "One or more validation errors occurred.";
        int errorStatusCode = ResolveStatusCodeForError(errorMessage);

        return StatusCode(errorStatusCode, ApiResponse.Failure(
            message: errorMessage,
            errors: result.Errors,
            statusCode: errorStatusCode));
    }

    private string GetControllerName()
    {
        return ControllerContext?.ActionDescriptor?.ControllerName ?? string.Empty;
    }

    private string GetActionName()
    {
        return ControllerContext?.ActionDescriptor?.ActionName ?? string.Empty;
    }

    private static string ResolveEmptyMessage(string controller, string action)
    {
        return (controller, action) switch
        {
            ("Admin", "GetReports") => "No moderation reports found.",
            ("Admin", "GetStats") => "No platform statistics available at this time.",
            ("Categories", "GetCategories") => "No categories found.",
            ("Chat", "GetConversations") => "No active conversations found.",
            ("Chat", "GetMessages") => "No messages found in this conversation.",
            ("Discover", "GetFeed") => "No community swappers found for your feed at this time.",
            ("Discover", "Search") => "No skills or members found matching your search query.",
            ("Discover", "GetRecommendedMatches") => "No reciprocal matches found matching your skill profile at this time.",
            ("Notifications", "GetNotifications") => "No notifications found.",
            ("Payments", "GetPaymentHistory") => "No payment transactions found.",
            ("Profile", "GetProfile") => "User profile not found.",
            ("Profile", "GetPublicUserProfile") => "Requested user profile was not found.",
            ("Reviews", "GetUserReviews") => "No reviews found for this user.",
            ("Sessions", "GetSessions") => "No swap sessions found matching the specified criteria.",
            ("Sessions", "GetById") => "Session details not found.",
            ("Sessions", "CallStatus") => "No call activity found for this session.",
            ("Sessions", "IceServers") => "No ICE servers configured.",
            ("Skills", "GetSkills") => "No skills found matching your search criteria.",
            ("SwapRequests", "GetIncoming") => "No incoming swap requests found.",
            ("SwapRequests", "GetOutgoing") => "No outgoing swap requests found.",

            _ when action.StartsWith("Search", StringComparison.OrdinalIgnoreCase) => "No records found matching your search query.",
            _ when action.StartsWith("Get", StringComparison.OrdinalIgnoreCase) => "No data found for the requested resource.",
            _ => "No records found matching your request."
        };
    }

    private static string ResolveSuccessMessage(string controller, string action)
    {
        return (controller, action) switch
        {
            ("Admin", "GetStats") => "Platform statistics retrieved successfully.",
            ("Admin", "GetReports") => "Moderation reports retrieved successfully.",
            ("Admin", "ResolveReport") => "Report resolved successfully.",
            ("Admin", "UpdateUserStatus") => "User status updated successfully.",

            ("Auth", "Register") => "User registered successfully.",
            ("Auth", "Login") => "Authentication successful.",
            ("Auth", "RefreshToken") => "Token refreshed successfully.",
            ("Auth", "Enable2Fa") => "Two-factor authentication setup initiated.",
            ("Auth", "Confirm2Fa") => "Two-factor authentication confirmed successfully.",
            ("Auth", "Verify2Fa") => "Two-factor verification successful.",
            ("Auth", "GoogleLogin") => "Google authentication successful.",
            ("Auth", "AppleLogin") => "Apple authentication successful.",
            ("Auth", "FacebookLogin") => "Facebook authentication successful.",
            ("Auth", "ForgotPassword") => "Password reset instructions sent successfully.",
            ("Auth", "ResetPassword") => "Password reset successfully.",

            ("Categories", "GetCategories") => "Categories retrieved successfully.",

            ("Chat", "GetConversations") => "Conversations retrieved successfully.",
            ("Chat", "GetMessages") => "Messages retrieved successfully.",
            ("Chat", "SendMessage") => "Message sent successfully.",

            ("Discover", "GetFeed") => "Discover feed retrieved successfully.",
            ("Discover", "Search") => "Search results retrieved successfully.",
            ("Discover", "GetRecommendedMatches") => "Recommended reciprocal matches retrieved successfully.",

            ("Notifications", "GetNotifications") => "Notifications retrieved successfully.",
            ("Notifications", "MarkAsRead") => "Notification marked as read successfully.",
            ("Notifications", "MarkAllAsRead") => "All notifications marked as read successfully.",

            ("Payments", "CreateDepositCheckout") => "Deposit checkout session created successfully.",
            ("Payments", "CreateSubscriptionCheckout") => "Subscription checkout session created successfully.",
            ("Payments", "GetPaymentHistory") => "Payment transaction history retrieved successfully.",

            ("Profile", "GetProfile") => "User profile retrieved successfully.",
            ("Profile", "UpdateProfile") => "Profile updated successfully.",
            ("Profile", "UploadAvatar") => "Avatar uploaded successfully.",
            ("Profile", "SetAvailability") => "Availability schedule updated successfully.",
            ("Profile", "GetPublicUserProfile") => "Public user profile retrieved successfully.",

            ("Reports", "CreateReport") => "Report submitted successfully.",

            ("Reviews", "CreateReview") => "Review submitted successfully.",
            ("Reviews", "GetUserReviews") => "User reviews retrieved successfully.",

            ("Sessions", "GetSessions") => "Swap sessions retrieved successfully.",
            ("Sessions", "GetById") => "Session details retrieved successfully.",
            ("Sessions", "Complete") => "Session marked as completed successfully.",
            ("Sessions", "Cancel") => "Session cancelled successfully.",
            ("Sessions", "Join") => "Joined video session successfully.",
            ("Sessions", "Leave") => "Left video session successfully.",
            ("Sessions", "Heartbeat") => "Session heartbeat acknowledged.",
            ("Sessions", "CallStatus") => "Session call telemetry retrieved successfully.",
            ("Sessions", "IceServers") => "WebRTC ICE servers retrieved successfully.",

            ("Skills", "GetSkills") => "Skills catalog retrieved successfully.",
            ("Skills", "AddOfferedSkill") => "Offered skill added to profile successfully.",
            ("Skills", "DeleteOfferedSkill") => "Offered skill removed from profile successfully.",
            ("Skills", "AddWantedSkill") => "Wanted skill added to profile successfully.",
            ("Skills", "DeleteWantedSkill") => "Wanted skill removed from profile successfully.",

            ("SwapRequests", "Create") => "Swap request created successfully.",
            ("SwapRequests", "GetIncoming") => "Incoming swap requests retrieved successfully.",
            ("SwapRequests", "GetOutgoing") => "Outgoing swap requests retrieved successfully.",
            ("SwapRequests", "Accept") => "Swap request accepted successfully.",
            ("SwapRequests", "Reject") => "Swap request rejected successfully.",
            ("SwapRequests", "CounterOffer") => "Counter proposal sent successfully.",

            _ when action.StartsWith("Get", StringComparison.OrdinalIgnoreCase) => "Data retrieved successfully.",
            _ when action.StartsWith("Create", StringComparison.OrdinalIgnoreCase) || action.StartsWith("Add", StringComparison.OrdinalIgnoreCase) => "Resource created successfully.",
            _ when action.StartsWith("Update", StringComparison.OrdinalIgnoreCase) || action.StartsWith("Set", StringComparison.OrdinalIgnoreCase) => "Resource updated successfully.",
            _ when action.StartsWith("Delete", StringComparison.OrdinalIgnoreCase) || action.StartsWith("Remove", StringComparison.OrdinalIgnoreCase) => "Resource deleted successfully.",
            _ => "Operation completed successfully."
        };
    }

    private static int ResolveStatusCodeForError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return StatusCodes.Status400BadRequest;

        var lower = errorMessage.ToLowerInvariant();
        if (lower.Contains("not found"))
            return StatusCodes.Status404NotFound;
        if (lower.Contains("unauthorized") || lower.Contains("not authorized") || lower.Contains("invalid credentials"))
            return StatusCodes.Status401Unauthorized;
        if (lower.Contains("forbidden") || lower.Contains("access denied") || lower.Contains("not allowed"))
            return StatusCodes.Status403Forbidden;
        if (lower.Contains("conflict") || lower.Contains("already exists"))
            return StatusCodes.Status409Conflict;

        return StatusCodes.Status400BadRequest;
    }
}
