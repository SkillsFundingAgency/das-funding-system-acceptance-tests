using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

internal class EarningsEntitySqlClient
{
    private readonly SqlServerClient _sqlServerClient;

    public EarningsEntitySqlClient()
    {
        var connectionString = Configurator.GetConfiguration().EarningsDbConnectionString;
        _sqlServerClient = SqlServerClientProvider.GetSqlServerClient(connectionString);
    }

    public EarningsApprenticeshipModel? GetEarningsEntityModel(ScenarioContext context)
    {
        var earningsEvent = context.Get<EarningsGeneratedEvent>();
        var apprenticeshipKey = earningsEvent.ApprenticeshipKey;

        var apprenticeship = _sqlServerClient.GetList<EarningsApprenticeshipModel>($"SELECT * FROM [Domain].[Apprenticeship] Where [key] ='{apprenticeshipKey}'").Single();
        var apprenticeshipEpisodes = _sqlServerClient.GetList<EpisodeModel>($"SELECT * FROM [Domain].[Episode] Where ApprenticeshipKey ='{apprenticeshipKey}'");

        foreach(var episode in apprenticeshipEpisodes)
        {
            episode.Prices = _sqlServerClient.GetList<EpisodePriceModel>($"SELECT * FROM [Domain].[EpisodePrice] Where EpisodeKey ='{episode.Key}'");
            episode.EarningsProfile = _sqlServerClient.GetList<EarningsProfileModel>($"SELECT * FROM [Domain].[EarningsProfile] Where EpisodeKey ='{episode.Key}'").Single();
            episode.EarningsProfile.Instalments = _sqlServerClient.GetList<InstalmentModel>($"SELECT Amount, AcademicYear, DeliveryPeriod FROM [Domain].[Instalment] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            episode.EarningsProfileHistory = _sqlServerClient.GetList<EarningsProfileHistoryModel>($"SELECT * FROM [Domain].[EarningsProfileHistory] Where EpisodeKey ='{episode.Key}'");
            
            foreach(var history in episode.EarningsProfileHistory)
            {
                history.Instalments = _sqlServerClient.GetList<InstalmentHistoryModel>($"SELECT * FROM [Domain].[Instalment] Where EarningsProfileId ='{history.EarningsProfileId}'");
            }

        }

        apprenticeship.Episodes = apprenticeshipEpisodes;

        return apprenticeship;
    }

}
