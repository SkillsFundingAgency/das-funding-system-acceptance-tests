using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

internal class ApprenticeshipsSqlClient
{
    private readonly SqlServerClient _sqlServerClient;

    public ApprenticeshipsSqlClient()
    {
        var connectionString = Configurator.GetConfiguration().ApprenticeshipsDbConnectionString;
        _sqlServerClient = SqlServerClientProvider.GetSqlServerClient(connectionString);
    }

    public void DeleteApprenticeship(Guid apprenticeshipKey)
    {
        _sqlServerClient.Execute($"DELETE FROM [dbo].[WithdrawalRequest] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");
        _sqlServerClient.Execute($"DELETE FROM [dbo].[StartDateChange] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");
        _sqlServerClient.Execute($"DELETE FROM [dbo].[PriceHistory] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");
        _sqlServerClient.Execute($"DELETE FROM [dbo].[FreezeRequest] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");

        var episodeKeys = _sqlServerClient.GetList<Guid>($"SELECT [Key] FROM [dbo].[Episode] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");

        foreach (var episodeKey in episodeKeys)
        {
            _sqlServerClient.Execute($"DELETE FROM [dbo].[EpisodePrice] WHERE EpisodeKey = '{episodeKey}'");
            _sqlServerClient.Execute($"DELETE FROM [dbo].[Episode] WHERE [Key] = '{episodeKey}'");
        }

        _sqlServerClient.Execute($"DELETE FROM [dbo].[Apprenticeship] WHERE [Key] = '{apprenticeshipKey}'");

    }


}
