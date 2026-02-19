using FluentAssertions.Equivalency;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

public class LearningSqlClient
{
    private readonly SqlServerClient _sqlServerClient;

    public LearningSqlClient()
    {
        var connectionString = Configurator.GetConfiguration().LearningDbConnectionString;
        _sqlServerClient = SqlServerClientProvider.GetSqlServerClient(connectionString);
    }


    public void DeleteApprenticeship(Guid learningKey)
    {
        var sql = $@"
            DELETE FROM [dbo].[FreezeRequest] WHERE LearningKey = '{learningKey}';
            DELETE FROM [dbo].[MathsAndEnglish] WHERE LearningKey = '{learningKey}';
            DELETE FROM [dbo].[LearningSupport] WHERE LearningKey = '{learningKey}';
        ";
        _sqlServerClient.Execute(sql);

        var episodeKeys = _sqlServerClient.GetList<Guid>($"SELECT [Key] FROM [dbo].[Episode] WHERE LearningKey = '{learningKey}'");

        foreach (var episodeKey in episodeKeys)
        {
            _sqlServerClient.Execute($"DELETE FROM [dbo].[EpisodePrice] WHERE EpisodeKey = '{episodeKey}'");
            _sqlServerClient.Execute($"DELETE FROM [dbo].[Episode] WHERE [Key] = '{episodeKey}'");
        }

        _sqlServerClient.Execute($"DELETE FROM [dbo].[Learning] WHERE [Key] = '{learningKey}'");
    }

    public Learning GetApprenticeship(Guid learningKey)
    {
        var learning = _sqlServerClient.GetList<Learning>("SELECT * FROM [dbo].[ApprenticeshipLearning] WHERE [KEY] = @learningKey", new { learningKey }).FirstOrDefault(); ;
        if (learning == null)
        {
            throw new InvalidOperationException("No learning found");
        }

        learning.Learner = _sqlServerClient.GetList<Learner>("SELECT * from [dbo].[Learner] WHERE [KEY] = @learnerKey", new { learnerKey = learning.LearnerKey }).FirstOrDefault();

        learning.Episodes = _sqlServerClient.GetList<Episode>($"SELECT * FROM [dbo].[Episode] WHERE LearningKey = '{learning.Key}'");

        foreach (var episode in learning.Episodes)
        {
            episode.Prices = _sqlServerClient.GetList<EpisodePrice>($"SELECT * FROM [dbo].[EpisodePrice] WHERE EpisodeKey = '{episode.Key}'");

            episode.EpisodeBreakInLearning = _sqlServerClient.GetList<EpisodeBreakInLearning>($" SELECT * FROM [dbo].[EpisodeBreakInLearning] WHERE EpisodeKey ='{episode.Key}'");

        }

        learning.FreezeRequests = _sqlServerClient.GetList<FreezeRequest>($"SELECT * FROM [dbo].[FreezeRequest] WHERE LearningKey = '{learning.Key}'");

        learning.LearningHistory = _sqlServerClient.GetList<LearningHistoryModel>($"SELECT * FROM [History].[LearningHistory] WHERE LearningId = '{learning.Key}'");

        return learning;
    }

    public List<Http.LearnerDataOuterApiClient.Learning> GetApprovedLearners(long ukprn, int academicYear)
    {
        var dates = AcademicYearParser.ParseFrom(academicYear);

        var learners = _sqlServerClient.GetList<Http.LearnerDataOuterApiClient.Learning>($"select distinct lrn.[Uln], lrn.[Key] " +
            $" from [dbo].[Learning] lrn " +
            $" inner join [dbo].[Episode] ep on ep.LearningKey = lrn.[Key] " +
            $" inner join [dbo].[EpisodePrice] eppr on eppr.EpisodeKey = ep.[Key] " +
            $" WHERE (eppr.StartDate <= '{dates.End}' AND eppr.EndDate   >= '{dates.Start}') " +
            $" AND ep.Ukprn = {ukprn} " +
            $" AND (ep.LastDayOfLearning is null " +
            $" OR ep.LastDayOfLearning >= '{dates.Start}' " +
            $" AND  ep.LastDayOfLearning <> eppr.StartDate)");


        return learners;
    }

    public void DeleteAllDataForUkprn(long ukprn)
    {
        var sql = @"

            /*===========================================================
            1. Delete Episode Prices
            ===========================================================*/
            DELETE ep
            FROM dbo.EpisodePrice ep
            JOIN dbo.Episode e ON ep.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            2. Delete Freeze Requests
            ===========================================================*/
            DELETE fr
            FROM dbo.FreezeRequest fr
            JOIN dbo.Learning l ON fr.LearningKey = l.[Key]
            JOIN dbo.Episode e ON l.[Key] = e.LearningKey
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            3. Delete Learning History 
            ===========================================================*/
            DELETE lh
            FROM History.LearningHistory lh
            JOIN dbo.Learning l ON lh.LearningId = l.[Key]
            JOIN dbo.Episode e ON l.[Key] = e.LearningKey
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            4. Delete Learning Supports
            ===========================================================*/
            DELETE ls
            FROM dbo.LearningSupport ls
            JOIN dbo.Learning l ON ls.LearningKey = l.[Key]
            JOIN dbo.Episode e ON ls.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            5. Delete Episode Breaks In Learning
            ===========================================================*/
            DELETE ebil
            FROM dbo.EpisodeBreakInLearning ebil
            JOIN dbo.Episode e ON ebil.EpisodeKey = e.[Key]
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            6. Delete Maths and English Breaks In Learning
            ===========================================================*/
            DELETE mebil
            FROM dbo.MathsAndEnglishBreakInLearning mebil
            JOIN dbo.MathsAndEnglish me on mebil.MathsAndEnglishKey = me.[Key]
            JOIN dbo.Learning l ON me.LearningKey = l.[Key]
            JOIN dbo.Episode e ON l.[Key] = e.LearningKey
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            7. Delete Maths and English
            ===========================================================*/
            DELETE me
            FROM dbo.MathsAndEnglish me
            JOIN dbo.Learning l ON me.LearningKey = l.[Key]
            JOIN dbo.Episode e ON l.[Key] = e.LearningKey
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            8. Delete Approvals
            ===========================================================*/
            DELETE a
            FROM dbo.Approval a
            JOIN dbo.Learning l ON a.ApprenticeshipKey = l.[Key]
            JOIN dbo.Episode e ON l.[Key] = e.LearningKey
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            9. Delete Episodes
            ===========================================================*/
            DELETE e
            FROM dbo.Episode e
            WHERE e.Ukprn = @Ukprn;

            /*===========================================================
            10. Delete Learnings
            ===========================================================*/
            DELETE l
            FROM dbo.Learning l
            WHERE EXISTS (
                SELECT 1 
                FROM dbo.Episode e 
                WHERE e.LearningKey = l.[Key]
                  AND e.Ukprn = @Ukprn
            );
        ";

        _sqlServerClient.Execute(sql, new { Ukprn = ukprn });
    }
}

public class Learning
{
    public Guid Key { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    //public string ApprenticeshipHashedId { get; set; } = null!; //todo: delete
    public DateTime? CompletionDate { get; set; } = null;
    public string? EmailAddress { get; set; }
    public List<FreezeRequest> FreezeRequests { get; set; } //todo: delete
    public List<Episode> Episodes { get; set; }
    public List<LearningHistoryModel> LearningHistory { get; set; }
    public Guid LearnerKey { get; set; }
    public Learner Learner { get; set; }
}

public class Learner
{
    public string Uln { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }

}

public class Episode
{
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public bool IsDeleted { get; set; }

    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public DAS.Learning.Types.FundingType FundingType { get; set; }
    public DAS.Learning.Types.FundingPlatform? FundingPlatform { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public string TrainingCode { get; set; } = null!;
    public string? TrainingCourseVersion { get; set; }
    public bool PaymentsFrozen { get; set; }
    public List<EpisodePrice> Prices { get; set; }
    public DateTime? LastDayOfLearning { get; set; }
    public DateTime? PauseDate { get; set; }
    public List<EpisodeBreakInLearning> EpisodeBreakInLearning { get; set; }
}

public class EpisodePrice
{
    public Guid Key { get; set; }
    public Guid EpisodeKey { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? TrainingPrice { get; set; }
    public decimal? EndPointAssessmentPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int FundingBandMaximum { get; set; }
}

public class FreezeRequest
{
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public string FrozenBy { get; set; } = null!;
    public DateTime FrozenDateTime { get; set; }
    public bool Unfrozen { get; set; }
    public DateTime? UnfrozenDateTime { get; set; }
    public string? UnfrozenBy { get; set; }
    public string? Reason { get; set; }
}

public class LearningHistoryModel
{
    public Guid LearningId { get; set; }
    public DateTime CreatedOn { get; set; }
    public string State { get; set; }
}

public class EpisodeBreakInLearning
{
    public Guid Key { get; set; }
    public Guid EpisodeKey { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime PriorPeriodExpectedEndDate { get; set; }
}