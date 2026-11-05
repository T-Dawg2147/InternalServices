using InternalServices.Auth.Admin;
using Xunit;

namespace InternalServices.Auth.Admin.Tests;

public sealed class AdminAccessGuardTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    public void WouldRemoveLastAdminAccess_ReturnsTrue_WhenAllAccessWouldBeRemoved(
        int currentAccessCount,
        int remainingAccessCount)
    {
        var result = AdminAccessGuard.WouldRemoveLastAdminAccess(
            currentAccessCount,
            remainingAccessCount);

        Assert.True(result);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    public void WouldRemoveLastAdminAccess_ReturnsFalse_WhenAccessWouldRemain(
        int currentAccessCount,
        int remainingAccessCount)
    {
        var result = AdminAccessGuard.WouldRemoveLastAdminAccess(
            currentAccessCount,
            remainingAccessCount);

        Assert.False(result);
    }

    [Fact]
    public void WouldRemoveLastAdminAccess_ReturnsFalse_WhenNoAdminAccessExistsYet()
    {
        var result = AdminAccessGuard.WouldRemoveLastAdminAccess(
            currentAccessCount: 0,
            remainingAccessCount: 0);

        Assert.False(result);
    }
}