namespace InternalServices.Auth.Admin;

internal static class AdminSql
{
    public const string GetUsers = """
		SELECT UserId, ExternalId, Username, Email, FirstName, LastName, IsActive, CreatedAt, LastLoginAt
		FROM auth.Users
		ORDER BY LastName, FirstName, UserId;
		""";

    public const string GetRoles = """
		SELECT RoleId, RoleName, Description
		FROM auth.Roles
		ORDER BY RoleName;
		""";

    public const string GetTools = """
		SELECT ToolId, ToolName, IsActive, ToolKey
		FROM auth.Tools
		ORDER BY ToolName;
		""";

    public const string GetPermissions = """
		SELECT p.PermissionId, p.ToolId, t.ToolKey, t.ToolName, p.PermissionName, p.Description
		FROM auth.Permissions p
		JOIN auth.Tools t ON t.ToolId = p.ToolId
		ORDER BY p.PermissionName;
		""";

    public const string GetUserRoleIds = """
		SELECT RoleId
		FROM auth.UserRoles
		WHERE UserId = @UserId
		ORDER BY RoleId;
		""";

    public const string GetRolePermissionIds = """
		SELECT PermissionId
		FROM auth.RolePermissions
		WHERE RoleId = @RoleId
		ORDER BY PermissionId;
		""";

    // TODO: Might make this CreateIfNotExists instead?
    public const string CreateRole = """
		INSERT INTO auth.Roles (RoleName, Description)
		VALUES (@RoleName, @Description)

		SELECT CAST(SCOPE_IDENTITY() AS INT);
		""";

    public const string UpdateRoleDescription = """
        UPDATE auth.Roles
        SET Description = @Description
        WHERE RoleId = @RoleId
        """;

    public const string AssignRoleToUser = """
		IF NOT EXISTS (
		SELECT 1 FROM auth.UserRoles WHERE UserId = @UserId AND RoleId = @RoleId
		)
		BEGIN
		INSERT INTO auth.UserRoles (UserId, RoleId, GrantedAt, GrantedByUserId)
		VALUES (@UserId, @RoleId, SYSUTCDATETIME(), @ActorUserId);
		END
		""";

    public const string RemoveRoleFromUser = """
		DELETE FROM auth.UserRoles
		WHERE UserId = @UserId AND RoleId = @RoleId
		""";

    public const string GrantPermissionToRole = """
		IF NOT EXISTS (
		SELECT 1 FROM auth.RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId
		)
		BEGIN
		INSERT INTO auth.RolePermissions (RoleId, PermissionId)
		VALUES (@RoleId, @PermissionId);
		END
		""";

    public const string RevokePermissionFromRole = """
		DELETE FROM auth.RolePermissions
		WHERE RoleId = @RoleId AND PermissionId  = @PermissionId;
		""";

    public const string SetUserActive = """
		UPDATE auth.Users
		SET IsActive = @IsActive,
		UpdatedAt = SYSUTCDATETIME()
		WHERE UserId = @UserId;
		""";

    public const string InsertAudit = """
		INSERT INTO audit.ActivityLog (OccurredAt, UserId, ToolId, Action, Details)
		VALUE (SYSUTCDATETIME(), @ActorUserId, @ToolId, @Action, @Details);
		""";

    public const string GetToolByToolKey = """
		SELECT ToolId
		FROM auth.Tools
		WHERE ToolKey = @ToolKey;
		""";

    public const string GetUserExternalId = """
		SELECT ExternalId
		FROM auth.Users
		WHERE UserId = @UserId
		""";
    
    public const string CreateTool = """
	     INSERT INTO auth.Tools (ToolKey, ToolName, IsActive)
	     VALUES (@ToolKey, @ToolName, 1);

	     SELECT CAST(SCOPE_IDENTITY() AS INT);
	     """;

    public const string SetToolActive = """
		UPDATE auth.Tools
		SET IsActive = @IsActive
		WHERE ToolId = @ToolId;
		""";

    public const string CreatePermission = """
       INSERT INTO auth.Permissions (ToolId, PermissionName, Description)
       VALUES (@ToolId, @PermissionName, @Description);

       SELECT CAST(SCOPE_IDENTITY() AS INT);
       """;

    public const string GetRecentAuditLog = """
        SELECT TOP (@Take)
            a.OccurredAt,
            a.UserId,
            u.Username AS ActorUsername,
            u.Email AS ActorEmail,
            u.FirstName AS ActorFirstName,
            u.LastName AS ActorLastName,
            t.ToolName,
            a.Action,
            a.Details
        FROM audit.ActivityLog a
        LEFT JOIN auth.Users u ON u.UserId = a.UserId
        LEFT JOIN auth.Tools t ON t.ToolId = a.ToolId
        ORDER BY a.OccurredAt DESC;
        """;

    public const string CountAdminAccessExcludingRole = """
        FROM auth.UserRoles ur
        	JOIN auth.RolePermissions rp ON rp.RoleId = ur.RoleId
        	JOIN auth.Permissions p on p.PermissionId = rp.PermissionId
        	JOIN auth.Tools t on t.ToolId = p.ToolId
        WHERE ur.UserId = @UserId
        	AND ur.RoleId <> @RoleId
        	AND t.ToolKey = @ToolKey
        	AND p.PermissionName = 'admin';
        """;
    
    public const string CountAdminAccessExcludingRolePermission = """
        SELECT COUNT(*)
        FROM auth.UserRoles ur
        	JOIN auth.RolePermissions rp ON rp.RoleId = ur.RoleId
        	JOIN auth.Permissions p ON p.PermissionId = rp.PermissionId
        	JOIN auth.Tools t ON t.ToolId = p.ToolId
        WHERE ur.UserId = @UserId
        	AND NOT (rp.RoleId = @RoleId AND rp.PermissionId = @PermissionId)
        	AND t.ToolKey = @ToolKey
        	AND p.PermissionName = N'admin';                                    
        """;
}