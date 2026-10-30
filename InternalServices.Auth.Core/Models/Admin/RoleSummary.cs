namespace InternalServices.Auth.Core.Models.Admin;

public sealed record RoleSummary(
    int RoleId,
    string RoleName,
    string? Description);