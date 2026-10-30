namespace InternalServices.Auth.Core.Models;

/// <summary>
/// The permissions one user holds in one tool, plus the user's database ID.
/// </summary>
public sealed class ToolPermissions
{
    private readonly HashSet<string> _permissions;

    public ToolPermissions(int userId, string toolKey, IEnumerable<string> permissions)
    {
        UserId = userId;
        ToolKey = toolKey;

        _permissions = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary> auth.UserId </summary>
    public int UserId { get; }

    /// <summary> The tool these permissions apply to. </summary>
    public string ToolKey { get; }

    /// <summary> All permission name held, e.g. "basic", "viewer", "admin" </summary>
    public HashSet<string> Permissions => _permissions;

    /// <summary> True if the user holds the named permission in this tool. </summary>
    public bool Has(string permissionName) => _permissions.Contains(permissionName);

    /// <summary> A user with no permissions at all (e.g. first sign-in, before roes are granted). </summary>
    public static ToolPermissions None(int userId, string toolKey) =>
        new(userId, toolKey, []);
}