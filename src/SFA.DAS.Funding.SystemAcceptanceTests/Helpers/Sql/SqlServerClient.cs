using Dapper;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

internal class SqlServerClient
{
    private string _connectionString;

    public SqlServerClient(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<T> GetList<T>(string sql, object? parameters = null)
    {
        List<T> result = new List<T>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            result = connection.Query<T>(sql, parameters).ToList();
            connection.Close();
        }

        return result;
    }

    public void Execute(string sql)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            connection.Execute(sql);
            connection.Close();
        }
    }
}

/// <summary>
/// In the absence of DI this is used to prevent multiple instances of the same SqlServerClient being created
/// </summary>
internal static class SqlServerClientProvider
{
    private static readonly ConcurrentDictionary<string, SqlServerClient> _sqlServerClients = new();

    internal static SqlServerClient GetSqlServerClient(string connectionString)
    {
        return _sqlServerClients.GetOrAdd(connectionString, cs => new SqlServerClient(cs));
    }
}

