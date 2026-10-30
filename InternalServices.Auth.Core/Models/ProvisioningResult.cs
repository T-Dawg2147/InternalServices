namespace InternalServices.Auth.Core.Models;

/// <summary>
/// The outcome of resolving a signed-in person to a row in auth.Users.
/// </summary>
/// <param name="UserId">auth.Users.UserId</param>
/// <param name="IsActive">False if the account has been deactivated. Deactivated users must be refused.</param>
/// <param name="WasCreated">True if this sign-in created the user row (first sign-in)</param>
public record ProvisioningResult(int UserId, bool IsActive, bool WasCreated);