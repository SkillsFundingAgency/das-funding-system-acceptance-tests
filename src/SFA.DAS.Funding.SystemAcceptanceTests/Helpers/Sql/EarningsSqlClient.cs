using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

internal class EarningsSqlClient
{
    private readonly SqlServerClient _sqlServerClient;

    public EarningsSqlClient()
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

        foreach (var episode in apprenticeshipEpisodes)
        {
            episode.Prices = _sqlServerClient.GetList<EpisodePriceModel>($"SELECT * FROM [Domain].[EpisodePrice] Where EpisodeKey ='{episode.Key}'");
            episode.EarningsProfile = _sqlServerClient.GetList<EarningsProfileModel>($"SELECT * FROM [Domain].[EarningsProfile] Where EpisodeKey ='{episode.Key}'").Single();
            episode.EarningsProfile.Instalments = _sqlServerClient.GetList<InstalmentModel>($"SELECT Amount, AcademicYear, DeliveryPeriod FROM [Domain].[Instalment] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            episode.EarningsProfileHistory = _sqlServerClient.GetList<EarningsProfileHistoryModel>($"SELECT * FROM [Domain].[EarningsProfileHistory] Where EpisodeKey ='{episode.Key}'");

            foreach (var history in episode.EarningsProfileHistory)
            {
                history.Instalments = _sqlServerClient.GetList<InstalmentHistoryModel>($"SELECT * FROM [Domain].[Instalment] Where EarningsProfileId ='{history.EarningsProfileId}'");
            }

        }

        apprenticeship.Episodes = apprenticeshipEpisodes;

        return apprenticeship;
    }

    public void DeleteEarnings(Guid apprenticeshipKey)
    {
        _sqlServerClient.Execute($"DELETE FROM [Query].[Earning] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");

        var episodeKeys = _sqlServerClient.GetList<Guid>($"SELECT [Key] FROM [Domain].[Episode] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");

        foreach(var episodeKey in episodeKeys)
        {
            _sqlServerClient.Execute($"DELETE FROM [Domain].[EpisodePrice] WHERE EpisodeKey = '{episodeKey}'");
            DeleteEarningProfile(episodeKey);
            DeleteEarningProfileHistory(episodeKey);
            _sqlServerClient.Execute($"DELETE FROM [Domain].[Episode] WHERE [Key] = '{episodeKey}'");
        }

        _sqlServerClient.Execute($"DELETE FROM [Domain].[Apprenticeship] WHERE [Key] = '{apprenticeshipKey}'");
    }

    private void DeleteEarningProfile(Guid episodeKey)
    {
        var earningProfileIds = _sqlServerClient.GetList<Guid>($"SELECT EarningsProfileId FROM [Domain].[EarningsProfile] WHERE EpisodeKey = '{episodeKey}'");
        foreach(var earningProfileId in earningProfileIds)
        {
            _sqlServerClient.Execute($"DELETE FROM [Domain].[Instalment] WHERE EarningsProfileId = '{earningProfileId}'");
            _sqlServerClient.Execute($"DELETE FROM [Domain].[EarningsProfile] WHERE EarningsProfileId = '{earningProfileId}'");
        }
    }

    private void DeleteEarningProfileHistory(Guid episodeKey)
    {
        var earningProfileHistoryIds = _sqlServerClient.GetList<Guid>($"SELECT EarningsProfileId FROM [Domain].[EarningsProfileHistory] WHERE EpisodeKey = '{episodeKey}'");
        foreach (var earningProfileHistoryId in earningProfileHistoryIds)
        {
            _sqlServerClient.Execute($"DELETE FROM [Domain].[InstalmentHistory] WHERE EarningsProfileId = '{earningProfileHistoryId}'");
            _sqlServerClient.Execute($"DELETE FROM [Domain].[EarningsProfileHistory] WHERE EarningsProfileId = '{earningProfileHistoryId}'");
        }
    }
}
