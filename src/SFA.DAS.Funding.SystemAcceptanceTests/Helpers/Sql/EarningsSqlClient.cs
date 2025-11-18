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

    public void DeleteAllDataForUkprn(long ukprn)
    {
        var sql = @"

            /*===========================================================
                1. Delete Maths & English Instalments
            ===========================================================*/
            DELETE mei
            FROM Domain.MathsAndEnglishInstalment AS mei
            JOIN Domain.MathsAndEnglish AS me ON mei.MathsAndEnglishKey = me.[Key]
            JOIN Domain.EarningsProfile AS ep ON me.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.Episode e ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
                2. Delete Maths & English
            ===========================================================*/
            DELETE me
            FROM Domain.MathsAndEnglish AS me
            JOIN Domain.EarningsProfile AS ep ON me.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.Episode e ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
                3. Delete Additional Payments
            ===========================================================*/
            DELETE ap
            FROM Domain.AdditionalPayment AS ap
            JOIN Domain.EarningsProfile ep ON ap.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.Episode e ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
                4. Delete Instalments
            ===========================================================*/
            DELETE i
            FROM Domain.Instalment AS i
            JOIN Domain.EarningsProfile ep ON i.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.Episode e ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
                5. Delete Episode Prices
            ===========================================================*/
            DELETE epc
            FROM Domain.EpisodePrice AS epc
            JOIN Domain.Episode e ON epc.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
                6. Delete Earnings Profile History
            ===========================================================*/
            DELETE eph
            FROM History.EarningsProfileHistory AS eph
            JOIN Domain.EarningsProfile ep ON eph.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.Episode e ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
                7. Delete Earnings Profiles
            ===========================================================*/
            DELETE ep
            FROM Domain.EarningsProfile ep
            JOIN Domain.Episode e ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
                8. Delete Episodes
            ===========================================================*/
            DELETE e
            FROM Domain.Episode e
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
                9. Delete Apprenticeships
                   Only those now orphaned by deleted Episodes
            ===========================================================*/
            DELETE a
            FROM Domain.Apprenticeship a
            WHERE NOT EXISTS (
                SELECT 1
                FROM Domain.Episode e
                WHERE e.ApprenticeshipKey = a.[Key]
            );
    ";

        _sqlServerClient.Execute(sql, new { Ukprn = ukprn });
    }

}
