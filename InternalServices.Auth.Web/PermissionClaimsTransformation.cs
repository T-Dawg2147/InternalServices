using System.Security.Claims;
using InternalServices.Auth.Core;
using InternalServices.Auth.Core.Abstractions;
using Microsoft.AspNetCore.Authentication;

namespace InternalServices.Auth.Web;

/// <summary>
/// Runs after sign-in. Looks the person up in InternalServices and adds
/// their permissions for this tool as claims.
/// </summary>
internal sealed class PermissionClaimsTransformation : IClaimsTransformation
{
    private readonly IPermissionService _permissions;
    private readonly AuthOptions _options;

    public PermissionClaimsTransformation(IPermissionService permissions, AuthOptions options)
    {
        _permissions = permissions;
        _options = options;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        // This whole method can be called more than once per request; DON'T repeat the work.
        if (principal.HasClaim(c => c.Type == PermissionClaims.Resolved))
            return principal;

        var user = ClaimsMapper.ToAuthenticatedUser(principal);
        if (user is null)
            return principal;

        var extra = new ClaimsIdentity();

        try
        {
            var result = await _permissions.GetPermissionsAsync(user, _options.ToolKey);
            
            extra.AddClaim(new Claim(PermissionClaims.UserId, result.UserId.ToString()));
            
            foreach (var permission in result.Permissions)
                extra.AddClaim(new Claim(PermissionClaims.Permission, permission));
        }
        catch (UnauthorizedAccessException ex)
        {
            extra.AddClaim(new Claim(PermissionClaims.Deactivated, "true"));
        }
        
        extra.AddClaim(new Claim(PermissionClaims.Resolved, "true"));

        var clone = principal.Clone();
        clone.AddIdentity(extra);
        return clone;
    }
}