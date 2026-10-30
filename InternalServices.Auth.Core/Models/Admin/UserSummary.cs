namespace InternalServices.Auth.Core.Models.Admin;

public sealed record UserSummary(
    int UserId,
    string ExternalId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt);