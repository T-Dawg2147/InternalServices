using Microsoft.AspNetCore.Authorization;

namespace InternalServices.Auth.Web;

/// <summary>
/// The user must hold this named permission.
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}