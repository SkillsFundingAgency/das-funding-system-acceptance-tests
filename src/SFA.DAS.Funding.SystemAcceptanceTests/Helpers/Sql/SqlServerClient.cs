using Dapper;
using Microsoft.Data.SqlClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

internal class SqlServerClient
{
    private string _connectionString;

    public SqlServerClient(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<T> GetList<T>(string sql)
    {
        List<T> result = new List<T>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            result = connection.Query<T>(sql).ToList();
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
    private static Dictionary<string, SqlServerClient> _sqlServerClients = new Dictionary<string, SqlServerClient>();

    internal static SqlServerClient GetSqlServerClient(string connectionString)
    {
        if (_sqlServerClients.TryGetValue(connectionString, out var client))
        {
            return client;
        }

        var newClient = new SqlServerClient(connectionString);
        _sqlServerClients.Add(connectionString, newClient);
        return newClient;
    }
}

