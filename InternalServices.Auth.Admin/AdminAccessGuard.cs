namespace InternalServices.Auth.Admin;

internal static class AdminAccessGuard
{
    internal static bool WouldRemoveLastAdminAccess(
        int currentAccessCount,
        int remainingAccessCount) =>
        currentAccessCount > 0 && remainingAccessCount == 0;
}