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

    public EarningsApprenticeshipModel? GetApprenticeshipEarningsEntityModel(ScenarioContext context)
    {
        var testData = context.Get<TestData>();
        var apprenticeshipKey = testData.LearningKey;

        var apprenticeship = _sqlServerClient.GetList<EarningsApprenticeshipModel>($"SELECT * FROM [Domain].[ApprenticeshipLearning] Where [LearningKey] ='{apprenticeshipKey}'").SingleOrDefault();
        if (apprenticeship == null)
            return null;

        var apprenticeshipEpisodes = _sqlServerClient.GetList<EpisodeModel>($"SELECT * FROM [Domain].[ApprenticeshipEpisode] Where LearningKey ='{apprenticeshipKey}'");

        foreach (var episode in apprenticeshipEpisodes)
        {
            episode.Prices = _sqlServerClient.GetList<EpisodePriceModel>($"SELECT * FROM [Domain].[ApprenticeshipEpisodePrice] Where EpisodeKey ='{episode.Key}'");
            episode.EarningsProfile = _sqlServerClient.GetList<EarningsProfileModel>($"SELECT * FROM [Domain].[ApprenticeshipEarningsProfile] Where EpisodeKey ='{episode.Key}'").Single();
            episode.EarningsProfile.Instalments = _sqlServerClient.GetList<InstalmentModel>($"SELECT EarningsProfileId, AcademicYear, DeliveryPeriod, Amount, EpisodePriceKey, IsPayable, Type FROM [Domain].[ApprenticeshipInstalment] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            episode.EarningsProfileHistory = _sqlServerClient.GetList<EarningsProfileHistoryModel>($"SELECT * FROM [History].[ApprenticeshipEarningsProfileHistory] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            episode.AdditionalPayments = _sqlServerClient.GetList<AdditionalPaymentsModel>($"SELECT * FROM [Domain].[ApprenticeshipAdditionalPayment] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            episode.MathsAndEnglish = _sqlServerClient.GetList<MathsAndEnglishModel>($"SELECT * FROM [Domain].[EnglishAndMaths] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            if (episode.MathsAndEnglishInstalments == null)
            {
                episode.MathsAndEnglishInstalments = new List<MathsAndEnglishInstalment>();
            }

            if (episode.MathsAndEnglishPeriodInLearning == null)
            {
                episode.MathsAndEnglishPeriodInLearning = new List<MathsAndEnglishPeriodInLearning>();
            }

            foreach (var learning in episode.MathsAndEnglish)
            {
                var instalments = _sqlServerClient.GetList<MathsAndEnglishInstalment>($"SELECT [Key], EnglishAndMathsKey AS MathsAndEnglishKey, AcademicYear, DeliveryPeriod, Amount, Type FROM [Domain].[EnglishAndMathsInstalment] WHERE EnglishAndMathsKey = '{learning.Key}'");
                var mathsAndEnglishPeriodInLearning = _sqlServerClient.GetList<MathsAndEnglishPeriodInLearning>($"SELECT [Key], EnglishAndMathsKey AS MathsAndEnglishKey, StartDate, EndDate, OriginalExpectedEndDate FROM [Domain].[EnglishAndMathsPeriodInLearning] WHERE EnglishAndMathsKey = '{learning.Key}'");

                if (instalments != null)
                {
                    episode.MathsAndEnglishInstalments.AddRange(instalments);
                }

                if (mathsAndEnglishPeriodInLearning != null)
                {
                    episode.MathsAndEnglishPeriodInLearning.AddRange(mathsAndEnglishPeriodInLearning);
                }

            }

            episode.EpisodePeriodInLearning = _sqlServerClient.GetList<EpisodePeriodInLearning>($" SELECT * FROM [Domain].[ApprenticeshipPeriodInLearning] WHERE EpisodeKey ='{episode.Key}'");
        }

        apprenticeship.Episodes = apprenticeshipEpisodes;

        return apprenticeship;
    }

    public ShortCourseEarningsModel? GetShortCourseEarningsEntityModel(string uln)
    {
        var shortCourse = _sqlServerClient.GetList<ShortCourseEarningsModel>($"SELECT * FROM [Domain].[ShortCourseLearning] Where [Uln] ='{uln}'").SingleOrDefault();
        if (shortCourse == null)
            return null;

        var episodes = _sqlServerClient.GetList<ShortCourseEpisodeModel>($"SELECT * FROM [Domain].[ShortCourseEpisode] Where LearningKey ='{shortCourse.LearningKey}'");

        foreach (var episode in episodes)
        {
            episode.EarningsProfile = _sqlServerClient.GetList<ShortCourseEarningsProfileModel>($"SELECT * FROM [Domain].[ShortCourseEarningsProfile] Where EpisodeKey ='{episode.Key}'").Single();
            episode.EarningsProfile.Instalments = _sqlServerClient.GetList<ShortCourseInstalmentModel>($"SELECT * FROM [Domain].[ShortCourseInstalment] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");
        }

        shortCourse.Episodes = episodes;

        return shortCourse;
    }

    public void DeleteEarnings(Guid apprenticeshipKey)
    {
        var episodeKeys = _sqlServerClient.GetList<Guid>($"SELECT [Key] FROM [Domain].[ApprenticeshipEpisode] WHERE LearningKey = '{apprenticeshipKey}'");

        foreach(var episodeKey in episodeKeys)
        {
            _sqlServerClient.Execute($"DELETE FROM [Domain].[ApprenticeshipEpisodePrice] WHERE EpisodeKey = '{episodeKey}'");
            _sqlServerClient.Execute($"DELETE FROM [Domain].[ApprenticeshipPeriodInLearning] WHERE EpisodeKey = '{episodeKey}'");

            DeleteEarningProfileHistory(episodeKey);
            DeleteEnglishAndMaths(episodeKey);
            DeleteEarningProfile(episodeKey);
            _sqlServerClient.Execute($"DELETE FROM [Domain].[ApprenticeshipEpisode] WHERE [Key] = '{episodeKey}'");
        }

        _sqlServerClient.Execute($"DELETE FROM [Domain].[ApprenticeshipLearning] WHERE [LearningKey] = '{apprenticeshipKey}'");
    }

    private void DeleteEarningProfile(Guid episodeKey)
    {
        var earningProfileIds = _sqlServerClient.GetList<Guid>($"SELECT EarningsProfileId FROM [Domain].[ApprenticeshipEarningsProfile] WHERE EpisodeKey = '{episodeKey}'");
        foreach(var earningProfileId in earningProfileIds)
        {
            _sqlServerClient.Execute($"DELETE FROM [Domain].[ApprenticeshipInstalment] WHERE EarningsProfileId = '{earningProfileId}'");
            _sqlServerClient.Execute($"DELETE FROM [Domain].[ApprenticeshipEarningsProfile] WHERE EarningsProfileId = '{earningProfileId}'");
        }
    }

    private void DeleteEarningProfileHistory(Guid episodeKey)
    {
        var earningProfileIds = _sqlServerClient.GetList<Guid>($"SELECT EarningsProfileId FROM [Domain].[ApprenticeshipEarningsProfile] WHERE EpisodeKey = '{episodeKey}'");

        if (earningProfileIds != null || earningProfileIds.Count > 0)
        {
            var profileId = string.Join(",", earningProfileIds.Select(k => $"'{k}'"));

            _sqlServerClient.Execute($"DELETE FROM [History].[ApprenticeshipEarningsProfileHistory] WHERE EarningsProfileId IN ({profileId})");

        }
    }

    public void DeleteEnglishAndMaths(Guid episodeKey)
    {
        var earningProfileIds = _sqlServerClient.GetList<Guid>($"SELECT EarningsProfileId FROM [Domain].[ApprenticeshipEarningsProfile] WHERE EpisodeKey = '{episodeKey}'");

        if (earningProfileIds == null || earningProfileIds.Count == 0)
            return;

        var profileIdList = string.Join(",", earningProfileIds.Select(id => $"'{id}'"));

        var mathsAndEnglishKeys = _sqlServerClient.GetList<Guid>($"SELECT [Key] FROM [Domain].[EnglishAndMaths] WHERE EarningsProfileId IN ({profileIdList})");

        if (mathsAndEnglishKeys != null && mathsAndEnglishKeys.Count > 0)
        {
            var keyList = string.Join(",", mathsAndEnglishKeys.Select(k => $"'{k}'"));

            _sqlServerClient.Execute($"DELETE FROM [Domain].[EnglishAndMathsInstalment] WHERE EnglishAndMathsKey IN ({keyList})");
            _sqlServerClient.Execute($"DELETE FROM [Domain].[EnglishAndMathsPeriodInLearning] WHERE EnglishAndMathsKey IN ({keyList})");
        }

        _sqlServerClient.Execute($"DELETE FROM [Domain].[EnglishAndMaths] WHERE EarningsProfileId IN ({profileIdList})");
    }

    public void DeleteAllDataForUkprn(long ukprn)
    {
        var sql = @"

            /*===========================================================
            1. Delete Maths & English Instalments
            ===========================================================*/
            DELETE mei
            FROM Domain.EnglishAndMathsInstalment AS mei
            JOIN Domain.EnglishAndMaths AS me 
                ON mei.EnglishAndMathsKey = me.[Key]
            JOIN Domain.ApprenticeshipEarningsProfile AS ep 
                ON me.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.ApprenticeshipEpisode e 
                ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            2. Delete Maths & English
            ===========================================================*/
            DELETE me
            FROM Domain.EnglishAndMaths AS me
            JOIN Domain.ApprenticeshipEarningsProfile AS ep 
                ON me.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.ApprenticeshipEpisode e 
                ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            3. Delete Additional Payments
            ===========================================================*/
            DELETE ap
            FROM Domain.ApprenticeshipAdditionalPayment AS ap
            JOIN Domain.ApprenticeshipEarningsProfile ep 
                ON ap.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.ApprenticeshipEpisode e 
                ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            4. Delete Instalments
            ===========================================================*/
            DELETE i
            FROM Domain.ApprenticeshipInstalment AS i
            JOIN Domain.ApprenticeshipEarningsProfile ep 
                ON i.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.ApprenticeshipEpisode e 
                ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            5. Delete Episode Prices
            ===========================================================*/
            DELETE epc
            FROM Domain.ApprenticeshipEpisodePrice AS epc
            JOIN Domain.ApprenticeshipEpisode e 
                ON epc.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            6. Delete Earnings Profile History
            ===========================================================*/
            DELETE eph
            FROM History.ApprenticeshipEarningsProfileHistory AS eph
            JOIN Domain.ApprenticeshipEarningsProfile ep 
                ON eph.EarningsProfileId = ep.EarningsProfileId
            JOIN Domain.ApprenticeshipEpisode e 
                ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            7. Delete Earnings Profiles
            ===========================================================*/
            DELETE ep
            FROM Domain.ApprenticeshipEarningsProfile ep
            JOIN Domain.ApprenticeshipEpisode e 
                ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            8. Delete Episode PeriodInLearning
            ===========================================================*/
            DELETE epil
            FROM Domain.ApprenticeshipPeriodInLearning epil
            JOIN Domain.ApprenticeshipEpisode e 
                ON epil.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            9. Delete Episodes
            ===========================================================*/
            DELETE e
            FROM Domain.ApprenticeshipEpisode e
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            10. Delete Learning records
                (Only those orphaned by deleted Episodes)
            ===========================================================*/
            DELETE l
            FROM Domain.ApprenticeshipLearning l
            WHERE NOT EXISTS (
                SELECT 1
                FROM Domain.ApprenticeshipEpisode e
                WHERE e.LearningKey = l.LearningKey
            );


            /*===========================================================
            11. Delete Short Course Instalments
            ===========================================================*/
            DELETE sci
            FROM Domain.ShortCourseInstalment AS sci
            JOIN Domain.ShortCourseEarningsProfile AS sep 
                ON sci.EarningsProfileId = sep.EarningsProfileId
            JOIN Domain.ShortCourseEpisode e 
                ON sep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            12. Delete Short Course Earnings Profile History
            ===========================================================*/
            DELETE seph
            FROM History.ShortCourseEarningsProfileHistory AS seph
            JOIN Domain.ShortCourseEarningsProfile sep 
                ON seph.EarningsProfileId = sep.EarningsProfileId
            JOIN Domain.ShortCourseEpisode e 
                ON sep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            13. Delete Short Course Earnings Profiles
            ===========================================================*/
            DELETE sep
            FROM Domain.ShortCourseEarningsProfile sep
            JOIN Domain.ShortCourseEpisode e 
                ON sep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            14. Delete Short Course Episodes
            ===========================================================*/
            DELETE e
            FROM Domain.ShortCourseEpisode e
            WHERE e.Ukprn = @Ukprn;


            /*===========================================================
            15. Delete Short Course Learning records
                (Only those orphaned by deleted Episodes)
            ===========================================================*/
            DELETE scl
            FROM Domain.ShortCourseLearning scl
            WHERE NOT EXISTS (
                SELECT 1
                FROM Domain.ShortCourseEpisode e
                WHERE e.LearningKey = scl.LearningKey
            );
        ";

        _sqlServerClient.Execute(sql, new { Ukprn = ukprn });
    }

}
