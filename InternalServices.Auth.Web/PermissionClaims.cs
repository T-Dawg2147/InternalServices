namespace InternalServices.Auth.Web;

/// <summary>
/// Claim type names this library adds to the signed-in user.
/// </summary>
public static class PermissionClaims
{
    /// <summary>
    /// One claim per permission held, e.g. value "basic"
    /// </summary>
    public const string Permission = "internalservices:permission";

    /// <summary>
    /// auth.Users.UserId as text.
    /// </summary>
    public const string UserId = "internalservices:userid";

    /// <summary>
    /// Present once permissions have been loaded, so we don't load twice.
    /// </summary>
    public const string Resolved = "internalservices:resolved";

    /// <summary>
    /// Present if the account exists but has been deactivated.
    /// </summary>
    public const string Deactivated = "internalservices:deactivated";
}