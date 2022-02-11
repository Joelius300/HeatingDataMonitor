using System.Data;

namespace HeatingDataMonitor.Database;

/// <summary>
/// Interface for opening database connections.
/// Intended for use with pooled connection providers.
/// </summary>
/// <typeparam name="TConnection"></typeparam>
public interface IConnectionProvider<TConnection>
    where TConnection : IDbConnection
{
    /// <summary>
    /// Returns an open and pooled connection to the database.
    /// Dispose after use!
    /// <para>
    /// You may call this method often as the management of the
    /// "physical" connection (which is performance critical) is
    /// handled by the ADO.NET data provider.
    /// </para>
    /// </summary>
    // TODO Add CancellationToken param?
    Task<TConnection> OpenConnection();
}
