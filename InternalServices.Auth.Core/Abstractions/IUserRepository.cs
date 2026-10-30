using InternalServices.Auth.Core.Models;

namespace InternalServices.Auth.Core.Abstractions;

/// <summary>
/// Reads and writes user records in the InternalServices database.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds the user by ExternalId. If they don't exist, creates them with NO roles.
    /// If they exist, refreshes name/email and LastLoginAt.
    /// </summary>
    Task<ProvisioningResult> ProvisionOnSignInAsync(
        AuthenticatedUser user, CancellationToken ct = default);

    /// <summary>
    /// Loads the permissions names a user holds in the given tool (via their roles).
    /// Returns an empty list if they hold none, or the tool is inactive or unknown.
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionsAsync(
        int userId, string toolKey, CancellationToken ct = default);
}