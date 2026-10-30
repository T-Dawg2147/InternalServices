using System.Collections.Concurrent;
using InternalServices.Auth.Core;
using InternalServices.Auth.Core.Abstractions;
using InternalServices.Auth.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace InternalServices.Auth.Data;

/// <inheritdoc/> 
internal sealed class PermissionService : IPermissionService
{
    private readonly IUserRepository _users;
    private readonly IMemoryCache _cache;
    private readonly AuthOptions _options;

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _keysByUser =
        new(StringComparer.OrdinalIgnoreCase);

    public PermissionService(IUserRepository users, IMemoryCache cache, AuthOptions options)
    {
        _users = users;
        _cache = cache;
        _options = options;
    }

    public async Task<ToolPermissions> GetPermissionsAsync(AuthenticatedUser user, string toolKey, CancellationToken ct = default)
    {
        var cacheKey = $"perm:{user.ExternalId}:{toolKey}";

        if (_cache.TryGetValue(cacheKey, out ToolPermissions? cached) && cached is not null)
            return cached;

        var provisioned = await _users.ProvisionOnSignInAsync(user, ct);

        if (!provisioned.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated.");

        var names = await _users.GetPermissionsAsync(provisioned.UserId, toolKey, ct);
        var result = new ToolPermissions(provisioned.UserId, toolKey, names);

        if (_options.PermissionCacheMinutes > 0)
        {
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_options.PermissionCacheMinutes));

            _keysByUser
                .GetOrAdd(user.ExternalId, _ => new ConcurrentDictionary<string, byte>())
                .TryAdd(cacheKey, 0);
        }

        return result;
    }

    public void Invalidate(string externalId)
    {
        if (_keysByUser.TryRemove(externalId, out var keys))
        {
            foreach (var key in keys.Keys)
                _cache.Remove(key);
        }
    }
}