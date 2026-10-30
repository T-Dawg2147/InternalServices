namespace InternalServices.Auth.Data;

/// <summary>
/// Every SQL statment user by the user repository.
/// Later, each of these will become store procedures,
/// </summary>
internal static class UserSql
{
	/// <summary>
	/// Find-or-create the user, refresh their details, and return the result.
	/// UPDLOCK + HOLDLOCK makes two simultaneous first sign-ins safe:
	/// the second waits for the first to finish rather than inserting a duplicate.
	/// </summary>
    public const string ProvisionOnSignIn = """
        SET XACT_ABORT ON;
        BEGIN TRANSACTION;
        
        DECLARE @UserId INT;
        DECLARE @IsActive BIT;
        DECLARE @WasCreated BIT = 0;
        
        SELECT @UserId = UserId, @IsActive = IsActive
        FROM auth.Users WITH (UPDLOCK, HOLDLOCK)
        WHERE ExternalId = @ExternalId;
        
        IF @UserId IS NULL
        BEGIN
        	INSERT INTO auth.Users (ExternalId, Username, Email, FirstName, LastName, LastLoginAt)
        	VALUES (@ExternalId, @Username, @Email, @FirstName, @LastName, SYSUTCDATETIME());
        	
        	SET @UserId		= SCOPE_IDENTITY();
        	SET @IsActive	= 1;
        	SET @WasCreated = 1;
        END
        ELSE
        BEGIN
        	UPDATE auth.Users
        	SET Username = @Username,
        		Email = @Email,
        		FirstName = @FirstName,
        		LastName = @LastName,
        		LastLoginAt = SYSUTCDATETIME(),
        		UpdatedAt = SYSUTCDATETIME()
        	WHERE UserId = @UserId;
        END
        
        COMMIT TRANSACTION;
        
        SELECT @UserId AS Userd, @IsActive AS IsActive, @WasCreated AS WasCreated;
        """;
    
    /// <summary>
    /// Permission names a user holds in one tool, via their roles.
    /// Inactive tools return nothing.
    /// </summary>
    public const string GetPermissions = """
        SELECT DISTINCT
        	p.PermissionName
        FROM auth.UserRoles       ur
        JOIN auth.RolePermissions rp ON rp.RoleId       = ur.RoleId
        JOIN auth.Permissions     p  ON p.PermissionId  = rp.PermissionId
        JOIN auth.Tools           t  ON t.ToolId        = p.ToolId
        WHERE ur.UserId = @UserId
            AND t.ToolKey = @ToolKey
            AND t.IsActive = 1;                                 
        """;
}