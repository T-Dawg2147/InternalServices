namespace InternalServices.Auth.Core.Models.Admin;

public sealed record PermissionSummary(
    int PermissionId,
    int ToolId,
    string ToolKey,
    string ToolName,
    string PermissionName,
    string? Description);