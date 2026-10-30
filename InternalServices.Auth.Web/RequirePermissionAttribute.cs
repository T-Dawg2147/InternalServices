using Microsoft.AspNetCore.Authorization;

namespace InternalServices.Auth.Web;

/// <summary>
/// Put on a page, controller or action.
/// The user must hold that permission in this app's tool.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "perm:";

    public RequirePermissionAttribute(string permission)
    {
        Policy = PolicyPrefix + permission;
    }
}