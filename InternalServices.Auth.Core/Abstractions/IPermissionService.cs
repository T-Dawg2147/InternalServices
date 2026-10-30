using InternalServices.Auth.Core.Models;

namespace InternalServices.Auth.Core.Abstractions;

/// <summary>
/// The main entry point for apps: "this person just signed in to this tool, what can they do?"
/// Handles provisioning, loading and short-term caching.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Resolves the signed-in person and returns their permissions for the tool.
    /// Throws <see cref="UnauthorizedAccessException"/> if the account is deactivated.
    /// </summary>
    Task<ToolPermissions> GetPermissionsAsync(
        AuthenticatedUser user, string toolKey, CancellationToken ct = default);

    /// <summary> Drops cached permissions for a user (call after changing their roles). </summary>
    void Invalidate(string externalId);
}