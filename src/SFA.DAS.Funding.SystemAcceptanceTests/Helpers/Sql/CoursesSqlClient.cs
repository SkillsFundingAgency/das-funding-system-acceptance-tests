using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

public class CoursesSqlClient
{
    private readonly SqlServerClient _sqlServerClient;

    public CoursesSqlClient()
    {
        var connectionString = Configurator.GetConfiguration().CoursesDbConnectionString;
        _sqlServerClient = SqlServerClientProvider.GetSqlServerClient(connectionString);
    }

    public void UpdateProposedMaxFunding(int value)
    {
        const string sql = "update [dbo].[ApprenticeshipFunding] set MaxEmployerLevyCap = @value where LarsCode = 'ZSC00005'";
        _sqlServerClient.Execute(sql, new { value });
        Console.WriteLine($"[CoursesSqlClient] Updated MaxEmployerLevyCap to {value} for LarsCode ZSC00005");
    }

    public void ResetProposedMaxFunding()
    {
        const string sql = "update [dbo].[ApprenticeshipFunding] set MaxEmployerLevyCap = 1000.00 where LarsCode = 'ZSC00005'";
        _sqlServerClient.Execute(sql);
        Console.WriteLine("[CoursesSqlClient] Reset MaxEmployerLevyCap to 1000 for LarsCode ZSC00005");
    }
}
