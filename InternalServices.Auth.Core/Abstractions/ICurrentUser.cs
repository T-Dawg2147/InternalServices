namespace InternalServices.Auth.Core.Abstractions;

/// <summary>
/// The signed-in user for the current request or session, as seen by tool code.
/// The Web package will provide an implementation backed by the HTTP request.
/// </summary>
public interface ICurrentUser
{
    /// <summary> True once the user has signed in and been resolved </summary>
    bool IsAuthenticated { get; }

    /// <summary> auth.UserId, or null if not signed in. </summary>
    int? UserId { get; }

    /// <summary> Display name, e.g. "Harry Calder-Ledgard", or null if not signed in. </summary>
    string? DisplayName { get; }

    /// <summary> True if user holds the names permission in this app's tool. </summary>
    bool HasPermission(string permissionName);
}