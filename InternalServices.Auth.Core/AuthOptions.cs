namespace InternalServices.Auth.Core;

/// <summary>
/// Settings every app supplies once when it registers the library.
/// </summary>
public sealed class AuthOptions
{
    /// <summary>
    /// Stable identifier of this tool, matching auth.Tools.ToolKey (e.g. admin-portal)
    /// </summary>
    public string ToolKey { get; set; } = string.Empty;

    /// <summary>
    /// Connection string to be the InternalService database. Service-side apps only.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary> How long permissions are cached per user, in minutes. Lower means role changes apply faster. </summary>
    public int PermissionCacheMinutes { get; set; } = 5;

    /// <summary>
    /// Throws if required settings are missing, so a misconfigured app fails at startup, not at first sign-in.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ToolKey))
            throw new InvalidOperationException($"{nameof(ToolKey)} must be set.");

        if (PermissionCacheMinutes < 0)
            throw new InvalidOperationException($"{nameof(PermissionCacheMinutes)} cannot be negative.");
    }
}