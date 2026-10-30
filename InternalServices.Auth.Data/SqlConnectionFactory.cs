using System.Data.Common;
using InternalServices.Auth.Core;
using Microsoft.Data.SqlClient;

namespace InternalServices.Auth.Data;

/// <inheritdoc/>
internal sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(AuthOptions options)
    {
        _connectionString = options.ConnectionString;
    }

    public async Task<DbConnection> OpenAsync(CancellationToken ct = default)
    {
        var connection = new SqlConnection(_connectionString);

        try
        {
            await connection.OpenAsync(ct);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }
}