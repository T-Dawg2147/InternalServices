using System.Data.Common;

namespace InternalServices.Auth.Data;

/// <summary>
/// Creates database connections.
/// Having this as an interfave means the repostiry never needs to know the connection string.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary> Creates and opens a new connection. The caller must dispose it. </summary>
    Task<DbConnection> OpenAsync(CancellationToken ct = default);
}