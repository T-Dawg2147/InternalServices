using System.Security.Claims;
using InternalServices.Auth.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace InternalServices.Auth.Web;

/// <summary>
/// ICurrentUser backed by the current HTTP request's user.
/// Good for controllers, minimal APIs and middleware. For Blazor components,
/// prefer AuthorizeView and [RequirePermission].
/// </summary>
internal sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public int? UserId =>
        int.TryParse(Principal?.FindFirst(PermissionClaims.UserId)?.Value, out var id) ? id : null;

    public string? DisplayName =>
        Principal?.FindFirst("name")?.Value ?? Principal?.Identity?.Name;

    public bool HasPermission(string permissionName) =>
        Principal?.Claims.Any(c =>
            c.Type == PermissionClaims.Permission &&
            string.Equals(c.Value, permissionName, StringComparison.OrdinalIgnoreCase)) == true;
}