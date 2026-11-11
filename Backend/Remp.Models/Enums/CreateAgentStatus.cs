namespace Remp.Models.Enums;

public enum CreateAgentStatus
{
    Success,
    CompanyNotFound,
    EmailAlreadyExists,
    UserCreationFailed,
    RoleAssignmentFailed,
    EmailSendingFailed,
    CompanyAssociationFailed
}