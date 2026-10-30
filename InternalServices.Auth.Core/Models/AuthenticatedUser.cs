namespace InternalServices.Auth.Core.Models;

/// <summary>
/// Identity details read from the Entra ID token after sign-in.
/// This is what the library receives; it is not yet a database user.
/// </summary>
/// <param name="ExternalId">The Entra ID. Matches auth.Users.ExternalId.</param>
/// <param name="Username">Usually the user principal name, e.g. harry.ledgard-calder@slingsby.com</param>
/// <param name="Email">The user's email address. e.g. harryc@slingsby.com</param>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Surname.</param>
public record AuthenticatedUser(
    string ExternalId,
    string Username,
    string Email,
    string FirstName,
    string LastName);