using Remp.DataAccess.Repositories;
using Remp.Models.Responses;
using Remp.Models.Requests;
using Microsoft.AspNetCore.Identity;
using Remp.Models.Entities;
using Remp.Models.Logs;
using Remp.Models.Results;
using Remp.Models.Enums;

namespace Remp.API.Services.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUpdateAccountPasswordLogRepository _updateAccountPasswordLogRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager, IUpdateAccountPasswordLogRepository updateAccountPasswordLogRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _updateAccountPasswordLogRepository = updateAccountPasswordLogRepository;
        _logger = logger;
    }

    public async Task<GetUsersResponse> GetUsersAsync (GetUsersRequest request)
    {
        int totalCount = await _userRepository.CountActiveUsersAsync();
        long skipCount = (long)(request.PageNumber - 1) * request.PageSize;
        List<UserResponse> users = new List<UserResponse>();
        if ( skipCount < totalCount)
        {
            users = await _userRepository.GetActiveUsersPageAsync((int)skipCount, request.PageSize);
        }

        GetUsersResponse response = new GetUsersResponse()
        {
            Users = users,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize

        };
        return response;
    }

    public async Task<GetCurrentUserResponse?> GetCurrentUserAsync(string userId, List<string> roles)
    {
        bool userExists = await _userRepository.ActiveUserExistsAsync(userId);
        if (!userExists)
        {
            return null;
        }

        List<int> assignedListingIds = await _userRepository.GetAssignedListingIdsAsync(userId);

        GetCurrentUserResponse response = new GetCurrentUserResponse()
        {
            UserId = userId,
            Roles = roles,
            AssignedListingIds = assignedListingIds
        };

        return response;

    }

    private async Task SaveUpdateAccountPasswordLogAsync(string userId, bool isSuccessful, string? failureReason)
    {
        try
        {
            UpdateAccountPasswordLog log = new UpdateAccountPasswordLog()
            {
                UserId = userId,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason
            };
            await _updateAccountPasswordLogRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save password update audit log for user {UserId}.", userId);
        }
    }

    public async Task<UpdateAccountPasswordResult> UpdateAccountPasswordAsync(string userId, UpdateAccountPasswordRequest request)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(userId);
        if(user == null || user.IsDeleted)
        {
            await SaveUpdateAccountPasswordLogAsync(userId, false, "UserNotFound");
            UpdateAccountPasswordResult result = new UpdateAccountPasswordResult()
            {
                Status = UpdateAccountPasswordStatus.UserNotFound
            };
            return result;
        }

        IdentityResult changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (changeResult.Succeeded == true)
        {
            await SaveUpdateAccountPasswordLogAsync(userId, true, null);
            UpdateAccountPasswordResult result = new UpdateAccountPasswordResult()
            {
                Status = UpdateAccountPasswordStatus.Success
            };
            return result;
        }

        bool isCurrentPasswordInvalid = changeResult.Errors.Any(error => error.Code == "PasswordMismatch");
        if(isCurrentPasswordInvalid == true)
        {
            await SaveUpdateAccountPasswordLogAsync(userId, false, "InvalidCurrentPassword");
            UpdateAccountPasswordResult result = new UpdateAccountPasswordResult()
            {
                Status = UpdateAccountPasswordStatus.InvalidCurrentPassword,
                ErrorMessage = "Current password is incorrect."
            };
            return result;
        }

        bool isNewPasswordInvalid = changeResult.Errors.Any(result => result.Code == "PasswordTooShort" || result.Code == "PasswordRequiresNonAlphanumeric" || result.Code == "PasswordRequiresDigit" || result.Code == "PasswordRequiresLower" || result.Code == "PasswordRequiresUpper" || result.Code == "PasswordRequiresUniqueChars");
        if (isNewPasswordInvalid)
        {
            await SaveUpdateAccountPasswordLogAsync(userId, false, "InvalidNewPassword");
            string errors = string.Join("; ", changeResult.Errors.Select(error => error.Description));
            UpdateAccountPasswordResult result = new UpdateAccountPasswordResult()
            {
                Status = UpdateAccountPasswordStatus.InvalidNewPassword,
                ErrorMessage = errors
            };
            return result;
        }

        await SaveUpdateAccountPasswordLogAsync(userId, false, "PasswordUpdateFailed");
        UpdateAccountPasswordResult failureResult = new UpdateAccountPasswordResult()
        {
            Status = UpdateAccountPasswordStatus.PasswordUpdateFailed,
            ErrorMessage = "Unable to update password. Please try again."
        };
        return failureResult;
    }


}