using Dapper;
using InternalServices.Auth.Core.Abstractions;
using InternalServices.Auth.Core.Models;

namespace InternalServices.Auth.Data;

/// <inheritdoc/>
internal sealed class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connections;

    public UserRepository(ISqlConnectionFactory connectionses)
    {
        _connections = connectionses;
    }

    public async Task<ProvisioningResult> ProvisionOnSignInAsync(AuthenticatedUser user, CancellationToken ct = default)
    {
        await using var connection = await _connections.OpenAsync(ct);

        var command = new CommandDefinition(
            UserSql.ProvisionOnSignIn,
            new
            {
                user.ExternalId,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName
            }, cancellationToken: ct);

        return await connection.QuerySingleAsync<ProvisioningResult>(command);
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(int userId, string toolKey, CancellationToken ct = default)
    {
        await using var connection = await _connections.OpenAsync(ct);

        var command = new CommandDefinition(
            UserSql.GetPermissions,
            new { UserId = userId, ToolKey = toolKey }, 
            cancellationToken: ct);

        var rows = await connection.QueryAsync<string>(command);
        return rows.ToList();
    }
}