using System.Security.Claims;
using InternalServices.Auth.Core.Models;

namespace InternalServices.Auth.Web;

internal static class ClaimsMapper
{
    // Microsoft.Identity.Web may expose the object ID under either name.
    private const string ObjectIdLong = "https://schemas.microsoft.com/identity/claims/objectidentifier";
    private const string ObjectIdShort = "iod";

    /// <summary>
    /// Returns null if the principal has no object ID,
    /// because without it, we cannot safely identify the person.
    /// </summary>
    public static AuthenticatedUser? ToAuthenticatedUser(ClaimsPrincipal principal)
    {
        var externalId = First(principal, ObjectIdLong, ObjectIdShort);
        if (string.IsNullOrWhiteSpace(externalId))
            return null;

        // Entra tokens often have no "email" claim, so fall back to the sign-in name.
        var username = First(principal, "preferred_username", ClaimTypes.Upn, "upn", ClaimTypes.Email)
                       ?? externalId;
        var email = First(principal, ClaimTypes.Email, "email", "preferred_username", "upn")
                    ?? username;

        var firstName = First(principal, ClaimTypes.GivenName, "given_name") ?? string.Empty;
        var lastName = First(principal, ClaimTypes.Surname, "family_name") ?? string.Empty;

        if (firstName.Length == 0 && lastName.Length == 0)
        {
            var display = First(principal, "name", ClaimTypes.Name) ?? username;
            firstName = display;
        }

        return new AuthenticatedUser(externalId, username, email, firstName, lastName);
    }

    private static string? First(ClaimsPrincipal principal, params string[] claimType)
    {
        foreach (var type in claimType)
        {
            var value = principal.FindFirst(type)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }
}