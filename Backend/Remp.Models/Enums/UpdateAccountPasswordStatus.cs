namespace Remp.Models.Enums;

public enum UpdateAccountPasswordStatus
{
    Success,
    UserNotFound,
    InvalidCurrentPassword,
    InvalidNewPassword,
    PasswordUpdateFailed
}