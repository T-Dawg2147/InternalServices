using InternalServices.Auth.Core.Models.Admin;

namespace InternalServices.Auth.Core.Abstractions;

public interface IAdminService
{
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RoleSummary>> GetRolesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ToolSummary>> GetToolsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PermissionSummary>> GetPermissionsAsync(CancellationToken ct = default);

    Task<IReadOnlyList<int>> GetUserRoleIdsAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetRolePermissionIdsAsync(int roleId, CancellationToken ct = default);

    Task<int> CreateRoleAsync(string roleName, string? description, int? actorUserId, CancellationToken ct = default);
    Task AssignRoleToUserAsync(int userId, int roleId, int? actorUserId, CancellationToken ct = default);
    Task RemoveRoleFromUserAsync(int userId, int roleId, int? actorUserId, CancellationToken ct = default);

    Task GrantPermissionToRoleAsync(int roleId, int permissionId, int? actorUserId, CancellationToken ct = default);
    Task RevokePermissionFromRoleAsync(int roleId, int permissionId, int? actorUserId, CancellationToken ct = default);

    Task SetUserActiveAsync(int userId, bool isActive, int? actorUserId, CancellationToken ct = default);

    Task UpdateRoleDescriptionAsync(int roleId, string? description, int? actorUserId, CancellationToken ct = default);
    
    Task<int> CreateToolAsync(string toolKey, string toolName, int? actorUserId, CancellationToken ct = default);
    Task SetToolActiveAsync(int toolId, bool isActive, int? actorUserId, CancellationToken ct = default);
    Task<int> CreatePermissionAsync(int toolId, string permissionName, string? description, int? actorUserId, CancellationToken ct = default);

    Task<IReadOnlyList<AuditLogEntry>> GetRecentAuditLogEntry(CancellationToken ct = default);
}