namespace InternalServices.Auth.Core.Models.Admin;

public sealed record ToolSummary(
    int ToolId,
    string ToolKey,
    string ToolName,
    bool IsActive);