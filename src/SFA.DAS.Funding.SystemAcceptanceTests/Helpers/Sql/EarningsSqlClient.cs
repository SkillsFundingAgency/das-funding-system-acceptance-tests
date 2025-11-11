using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

public class EarningsSqlClient
{
    private readonly SqlServerClient _sqlServerClient;

    public EarningsSqlClient()
    {
        var connectionString = Configurator.GetConfiguration().EarningsDbConnectionString;
        _sqlServerClient = SqlServerClientProvider.GetSqlServerClient(connectionString);
    }

    public EarningsApprenticeshipModel? GetEarningsEntityModel(ScenarioContext context)
    {
        var testData = context.Get<TestData>();
        var apprenticeshipKey = testData.LearningKey;

        var apprenticeship = _sqlServerClient.GetList<EarningsApprenticeshipModel>($"SELECT * FROM [Domain].[Apprenticeship] Where [key] ='{apprenticeshipKey}'").SingleOrDefault();
        if (apprenticeship == null)
            return null;

        var apprenticeshipEpisodes = _sqlServerClient.GetList<EpisodeModel>($"SELECT * FROM [Domain].[Episode] Where ApprenticeshipKey ='{apprenticeshipKey}'");

        foreach (var episode in apprenticeshipEpisodes)
        {
            episode.Prices = _sqlServerClient.GetList<EpisodePriceModel>($"SELECT * FROM [Domain].[EpisodePrice] Where EpisodeKey ='{episode.Key}'");
            episode.EarningsProfile = _sqlServerClient.GetList<EarningsProfileModel>($"SELECT * FROM [Domain].[EarningsProfile] Where EpisodeKey ='{episode.Key}'").Single();
            episode.EarningsProfile.Instalments = _sqlServerClient.GetList<InstalmentModel>($"SELECT EarningsProfileId, AcademicYear, DeliveryPeriod, Amount, EpisodePriceKey, Type, IsAfterLearningEnded FROM [Domain].[Instalment] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            episode.EarningsProfileHistory = _sqlServerClient.GetList<EarningsProfileHistoryModel>($"SELECT * FROM [History].[EarningsProfileHistory] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            episode.AdditionalPayments = _sqlServerClient.GetList<AdditionalPaymentsModel>($"SELECT * FROM [Domain].[AdditionalPayment] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            episode.MathsAndEnglish = _sqlServerClient.GetList<MathsAndEnglishModel>($"SELECT * FROM [Domain].[MathsAndEnglish] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            if (episode.MathsAndEnglishInstalments == null)
            {
                episode.MathsAndEnglishInstalments = new List<MathsAndEnglishInstalment>();
            }

            foreach (var learning in episode.MathsAndEnglish)
            {
                var instalments = _sqlServerClient.GetList<MathsAndEnglishInstalment>($"SELECT * FROM [Domain].[MathsAndEnglishInstalment] WHERE MathsAndEnglishKey = '{learning.Key}'");

                if (instalments != null)
                {
                    episode.MathsAndEnglishInstalments.AddRange(instalments);
                }
            }
        }

        apprenticeship.Episodes = apprenticeshipEpisodes;

        return apprenticeship;
    }

    public void DeleteEarnings(Guid apprenticeshipKey)
    {
        var episodeKeys = _sqlServerClient.GetList<Guid>($"SELECT [Key] FROM [Domain].[Episode] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");

        foreach(var episodeKey in episodeKeys)
        {
            _sqlServerClient.Execute($"DELETE FROM [Domain].[EpisodePrice] WHERE EpisodeKey = '{episodeKey}'");

            DeleteEarningProfileHistory(episodeKey);
            DeleteEnglishAndMaths(episodeKey);
            DeleteEarningProfile(episodeKey);
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
        var earningProfileIds = _sqlServerClient.GetList<Guid>($"SELECT EarningsProfileId FROM [Domain].[EarningsProfile] WHERE EpisodeKey = '{episodeKey}'");

        if (earningProfileIds != null || earningProfileIds.Count > 0)
        {
            var profileId = string.Join(",", earningProfileIds.Select(k => $"'{k}'"));

            _sqlServerClient.Execute($"DELETE FROM [History].[EarningsProfileHistory] WHERE EarningsProfileId IN ({profileId})");

        }
    }

    public void DeleteEnglishAndMaths(Guid episodeKey)
    {
        var earningProfileIds = _sqlServerClient.GetList<Guid>($"SELECT EarningsProfileId FROM [Domain].[EarningsProfile] WHERE EpisodeKey = '{episodeKey}'");

        if (earningProfileIds == null || earningProfileIds.Count == 0)
            return;

        var profileIdList = string.Join(",", earningProfileIds.Select(id => $"'{id}'"));

        var mathsAndEnglishKeys = _sqlServerClient.GetList<Guid>($"SELECT [Key] FROM [Domain].[MathsAndEnglish] WHERE EarningsProfileId IN ({profileIdList})");

        if (mathsAndEnglishKeys != null && mathsAndEnglishKeys.Count > 0)
        {
            var keyList = string.Join(",", mathsAndEnglishKeys.Select(k => $"'{k}'"));

            _sqlServerClient.Execute($"DELETE FROM [Domain].[MathsAndEnglishInstalment] WHERE MathsAndEnglishKey IN ({keyList})");
        }

        _sqlServerClient.Execute($"DELETE FROM [Domain].[MathsAndEnglish] WHERE EarningsProfileId IN ({profileIdList})");
    }

}
