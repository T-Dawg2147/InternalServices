using System.Data.Common;
using System.Text.RegularExpressions;
using Dapper;
using InternalServices.Auth.Core;
using InternalServices.Auth.Core.Abstractions;
using InternalServices.Auth.Core.Models.Admin;
using InternalServices.Auth.Data;
using Microsoft.Data.SqlClient;

namespace InternalServices.Auth.Admin;

internal sealed class AdminService : IAdminService
{
    private readonly ISqlConnectionFactory _connections;
    private readonly IPermissionService _permissionService;
    private readonly AuthOptions _options;
    
    public AdminService(
        ISqlConnectionFactory connections,
        IPermissionService permissionService,
        AuthOptions options)
    {
        _connections = connections;
        _permissionService = permissionService;
        _options = options;
    }

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);
        var cmd = new CommandDefinition(AdminSql.GetUsers , cancellationToken: ct);
        return (await conn.QueryAsync<UserSummary>(cmd)).ToList();
    }

    public async Task<IReadOnlyList<RoleSummary>> GetRolesAsync(CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);
        var cmd = new CommandDefinition(AdminSql.GetRoles , cancellationToken: ct);
        return (await conn.QueryAsync<RoleSummary>(cmd)).ToList();
    }

    public async Task<IReadOnlyList<ToolSummary>> GetToolsAsync(CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);
        var cmd = new CommandDefinition(AdminSql.GetTools , cancellationToken: ct);
        return (await conn.QueryAsync<ToolSummary>(cmd)).ToList();
    }

    public async Task<IReadOnlyList<PermissionSummary>> GetPermissionsAsync(CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);
        var cmd = new CommandDefinition(AdminSql.GetPermissions, cancellationToken: ct);
        return (await conn.QueryAsync<PermissionSummary>(cmd)).ToList();
    }

    public async Task<IReadOnlyList<int>> GetUserRoleIdsAsync(int userId, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);
        var cmd = new CommandDefinition(AdminSql.GetUserRoleIds,
            new { UserId = userId }, cancellationToken: ct);
        return (await conn.QueryAsync<int>(cmd)).ToList();
    }

    public async Task<IReadOnlyList<int>> GetRolePermissionIdsAsync(int roleId, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);
        var cmd = new CommandDefinition(AdminSql.GetRolePermissionIds, 
            new { RoleId = roleId }, cancellationToken: ct);
        return (await conn.QueryAsync<int>(cmd)).ToList();
    }

    public async Task<int> CreateRoleAsync(string roleName, string? description, int? actorUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name is required.", nameof(roleName));
        
        await using var conn = await _connections.OpenAsync(ct);

        var roleId = await conn.QuerySingleAsync<int>(
            new CommandDefinition(AdminSql.CreateRole, new
            {
                RoleName = roleName.Trim(),
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
            }, cancellationToken: ct));

        await WriteAuditAsync(conn, actorUserId, "RoleCreated", $"RoleId={roleId};RoleName={roleName.Trim()}", ct);
        return roleId;
    }

    public async Task UpdateRoleDescriptionAsync(int roleId, string? description, int? actorUserId, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);

        await conn.ExecuteAsync(new CommandDefinition(
            AdminSql.UpdateRoleDescription,
            new
            {
                RoleId = roleId,
                Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
            },
            cancellationToken: ct));

        await WriteAuditAsync(conn, actorUserId, "RoleDescriptionUpdated", $"RoleId={roleId}", ct);
    }

    public async Task<int> CreateToolAsync(string toolKey, string toolName, int? actorUserId, CancellationToken cancellationToken = default)
    {
        toolKey = (toolKey ?? string.Empty).Trim();
        toolName = (toolName ?? string.Empty).Trim();

        if (toolName.Length == 0)
            throw new ArgumentException("Tool name is required.", nameof(toolName));

        // Same rule as the CHECK constraint in 002_AddToolKey.sql
        if (!Regex.IsMatch(toolKey, "^[a-z0-9-]+$"))
            throw new ArgumentException("Tool key may only contain lowercase letters, digits and hyphens.", nameof(toolKey));

        await using var conn = await _connections.OpenAsync(cancellationToken);

        try
        {
            var toolId = await conn.QuerySingleAsync<int>(new CommandDefinition(
                AdminSql.CreateTool,
                new { ToolKey = toolKey, ToolName = toolName },
                cancellationToken: cancellationToken));

            await WriteAuditAsync(conn, actorUserId, "ToolCreated", $"ToolId={toolId};ToolKey={toolKey}", cancellationToken);
            return toolId;
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new InvalidOperationException("A tool with that key or name already exists.");
        }
    }

    public async Task SetToolActiveAsync(int toolId, bool isActive, int? actorUserId, CancellationToken cancellationToken = default)
    {
        await using var conn = await _connections.OpenAsync(cancellationToken);

        await conn.ExecuteAsync(new CommandDefinition(
            AdminSql.SetToolActive,
            new { ToolId = toolId, IsActive = isActive },
            cancellationToken: cancellationToken));

        await WriteAuditAsync(conn, actorUserId, isActive ? "ToolActivated" : "ToolDeactivated", $"ToolId={toolId}", cancellationToken);
    }

    public async Task<int> CreatePermissionAsync(int toolId, string permissionName, string? description, int? actorUserId, CancellationToken cancellationToken = default)
    {
        permissionName = (permissionName ?? string.Empty).Trim();

        if (permissionName.Length == 0)
            throw new ArgumentException("Permission name is required.", nameof(permissionName));

        await using var conn = await _connections.OpenAsync(cancellationToken);

        try
        {
            var permissionId = await conn.QuerySingleAsync<int>(new CommandDefinition(
                AdminSql.CreatePermission,
                new
                {
                    ToolId = toolId,
                    PermissionName = permissionName,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim()
                },
                cancellationToken: cancellationToken));

            await WriteAuditAsync(conn, actorUserId, "PermissionCreated", $"PermissionId={permissionId};ToolId={toolId};Name={permissionName}", cancellationToken);
            return permissionId;
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new InvalidOperationException("That tool already has a permission with that name.");
        }
    }

    public async Task AssignRoleToUserAsync(int userId, int roleId, int? actorUserId, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);

        await conn.ExecuteAsync(new CommandDefinition(
            AdminSql.AssignRoleToUser,
            new { UserId = userId, RoleId = roleId, ActorUserId = actorUserId },
            cancellationToken: ct));

        await InvalidateUserCacheAsync(conn, userId, ct);
        await WriteAuditAsync(conn, actorUserId, "UserRoleAssigned", $"UserId={userId};RoleId={roleId}", ct);
    }

    public async Task RemoveRoleFromUserAsync(int userId, int roleId, int? actorUserId, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);

        await EnsureAnActiveAdminRemainsAsync(
            conn, AdminSql.CountActiveAdminsAfterRoleRemoval,
            new { UserId = userId, RoleId = roleId, ToolKey = _options.ToolKey },
            ct);

        if (actorUserId == userId)
        {
            var before = await CountAdminAccessAsync(conn, userId, -1, -1, ct);
            var after = await CountAdminAccessAsync(conn, userId, roleId, null, ct);
            if (before > 0 && after == 0)
                throw new InvalidOperationException("You can't remove the role that gives you Admin access.");
        }
        
        await conn.ExecuteAsync(new CommandDefinition(
            AdminSql.RemoveRoleFromUser,
            new { UserId = userId, RoleId = roleId },
            cancellationToken: ct));

        await InvalidateUserCacheAsync(conn, userId, ct);
        await WriteAuditAsync(conn, actorUserId, "UserRoleRemoved", $"UserId={userId};RoleId={roleId}", ct);
    }

    public async Task GrantPermissionToRoleAsync(int roleId, int permissionId, int? actorUserId, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);
        
        await conn.ExecuteAsync(new CommandDefinition(
            AdminSql.GrantPermissionToRole,
            new { RoleId = roleId, PermissionId = permissionId },
            cancellationToken: ct));

        await WriteAuditAsync(conn, actorUserId, "RolePermissionGranted", $"RoleId={roleId};PermissionId={permissionId}", ct);
    }

    public async Task RevokePermissionFromRoleAsync(int roleId, int permissionId, int? actorUserId, CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);
        
        await EnsureAnActiveAdminRemainsAsync(
            conn,
            AdminSql.CountActiveAdminsAfterPermissionRevocation,
            new { RoleId = roleId, PermissionId = permissionId, ToolKey = _options.ToolKey },
            ct);
        
        if (actorUserId is int actor)
        {
            var before = await CountAdminAccessAsync(conn, actor, -1, -1, ct);
            var after = await CountAdminAccessAsync(conn, actor, roleId, permissionId, ct);
            if (before > 0 && after == 0)
                throw new InvalidOperationException("You can't revoke the permission that gives you Admin access.");
        }
        
        await conn.ExecuteAsync(new CommandDefinition(
            AdminSql.RevokePermissionFromRole,
            new { RoleId = roleId, PermissionId = permissionId },
            cancellationToken: ct));

        await WriteAuditAsync(conn, actorUserId, "RolePermissionRevoked", $"RoleId={roleId};PermissionId={permissionId}", ct);
    }

    public async Task SetUserActiveAsync(int userId, bool isActive, int? actorUserId, CancellationToken ct = default)
    {
        if (!isActive && actorUserId == userId)
            throw new InvalidOperationException("You can't deactivate you own account.");
        
        await using var conn = await _connections.OpenAsync(ct);
        
        if (!isActive)
        {
            await EnsureAnActiveAdminRemainsAsync(
                conn,
                AdminSql.CountActiveAdminsAfterDeactivation,
                new { UserId = userId, ToolKey = _options.ToolKey },
                ct);
        }
        
        await conn.ExecuteAsync(new CommandDefinition(
            AdminSql.SetUserActive,
            new { UserId = userId, IsActive = isActive },
            cancellationToken: ct));

        await InvalidateUserCacheAsync(conn, userId, ct);
        await WriteAuditAsync(conn, actorUserId, isActive ? "UserActivated" : "UserDeactivated", $"UserId={userId}", ct);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetRecentAuditLogEntry(CancellationToken ct = default)
    {
        await using var conn = await _connections.OpenAsync(ct);

        var cmd = new CommandDefinition(
            AdminSql.GetRecentAuditLog,
            new { Take = 200 },
            cancellationToken: ct);

        return (await conn.QueryAsync<AuditLogEntry>(cmd)).ToList();
    }

    private async Task WriteAuditAsync(
        DbConnection conn,
        int? actorUserId,
        string action,
        string? details,
        CancellationToken ct)
    {
        var toolId = await ResolveAdminToolIdAsync();

        await conn.ExecuteAsync(new CommandDefinition(
            AdminSql.InsertAudit,
            new
            {
                ActorUserId = actorUserId,
                ToolId = toolId,
                Action = action,
                Details = details
            },
            cancellationToken: ct));
    }

    private async Task<int?> ResolveAdminToolIdAsync()
    {
        await using var conn = await _connections.OpenAsync(CancellationToken.None);
        return await conn.QuerySingleOrDefaultAsync<int?>(new CommandDefinition(
            AdminSql.GetToolByToolKey,
            new { ToolKey = _options.ToolKey }));
    }

    private async Task EnsureAnActiveAdminRemainsAsync(
        DbConnection conn,
        string afterChangeSql,
        object afterChangeParameters,
        CancellationToken ct)
    {
        var before = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            AdminSql.CountActiveAdmins,
            new { ToolKey = _options.ToolKey },
            cancellationToken: ct));

        var after = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            afterChangeSql,
            afterChangeParameters,
            cancellationToken: ct));

        if (before > 0 && after == 0)
        {
            throw new InvalidOperationException(
                "This change would remove the last active user with Admin access to this tool.");
        }
    }

    private async Task InvalidateUserCacheAsync(DbConnection conn, int userId, CancellationToken ct)
    {
        var externalId = await conn.QuerySingleOrDefaultAsync<string?>(new CommandDefinition(
            AdminSql.GetUserExternalId,
            new { UserId = userId },
            cancellationToken: ct));

        if (!string.IsNullOrWhiteSpace(externalId))
            _permissionService.Invalidate(externalId);
    }

    private async Task<int> CountAdminAccessAsync(DbConnection conn, int userId, int roleId, int? permissionId,
        CancellationToken ct)
    {
        var sql = permissionId is null
            ? AdminSql.CountAdminAccessExcludingRole
            : AdminSql.CountAdminAccessExcludingRolePermission;

        return await conn.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new { UserId = userId, RoleId = roleId, PermissionId = permissionId, ToolKey = _options.ToolKey },
            cancellationToken: ct));
    }
}