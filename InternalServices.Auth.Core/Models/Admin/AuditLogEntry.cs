namespace InternalServices.Auth.Core.Models.Admin;

public sealed record AuditLogEntry(
    DateTime OccurredAt,
    int? UserId,
    string? ActorUsername,
    string? ActorEmail,
    string? ActorFirstName,
    string? ActorLastName,
    string? ToolName,
    string Action,
    string? Details);