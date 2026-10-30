using System.Security.Claims;
using InternalServices.Auth.Web;
using Microsoft.AspNetCore.Components.Authorization;

namespace Test_AdminPortal.Services;

public sealed class UserContext
{
    private readonly AuthenticationStateProvider _provider;

    public UserContext(AuthenticationStateProvider provider)
    {
        _provider = provider;
    }

    private async Task<ClaimsPrincipal> GetPrincipalAsync()
        => (await _provider.GetAuthenticationStateAsync()).User;

    public async Task<int?> GetUserIdAsync()
    {
        var principal = await GetPrincipalAsync();
        return int.TryParse(principal.FindFirst(PermissionClaims.UserId)?.Value, out var id) ? id : null;
    }

    public async Task<string> GetDisplayNameAsync()
    {
        var principal = await GetPrincipalAsync();
        return principal.FindFirst("name")?.Value ?? principal.Identity?.Name ?? string.Empty;
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync()
    {
        var principal = await GetPrincipalAsync();
        return principal.FindAll(PermissionClaims.Permission)
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }
}