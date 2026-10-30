using Microsoft.AspNetCore.Authorization;

namespace InternalServices.Auth.Web;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var hasIt = context.User.Claims.Any(c =>
            c.Type == PermissionClaims.Permission &&
            string.Equals(c.Value, requirement.Permission, StringComparison.OrdinalIgnoreCase));

        if (hasIt)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}