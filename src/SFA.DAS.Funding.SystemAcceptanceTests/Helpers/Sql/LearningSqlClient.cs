using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;

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
            DELETE FROM [dbo].[WithdrawalRequest] WHERE LearningKey = '{learningKey}';
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
        var learning = _sqlServerClient.GetList<Learning>("SELECT * FROM [dbo].[Learning] WHERE [KEY] = @learningKey", new { learningKey }).FirstOrDefault(); ;
        if (learning == null)
        {
            throw new InvalidOperationException("No learning found");
        }
        learning.Episodes = _sqlServerClient.GetList<Episode>($"SELECT * FROM [dbo].[Episode] WHERE LearningKey = '{learning.Key}'");
        foreach (var episode in learning.Episodes)
        {
            episode.Prices = _sqlServerClient.GetList<EpisodePrice>($"SELECT * FROM [dbo].[EpisodePrice] WHERE EpisodeKey = '{episode.Key}'");
        }
        learning.FreezeRequests = _sqlServerClient.GetList<FreezeRequest>($"SELECT * FROM [dbo].[FreezeRequest] WHERE LearningKey = '{learning.Key}'");
        learning.WithdrawalRequests = _sqlServerClient.GetList<WithdrawalRequest>($"SELECT * FROM [dbo].[WithdrawalRequest] WHERE LearningKey = '{learning.Key}'");
        return learning;
    }

    public List<Http.LearnerDataOuterApiClient.Learning> GetApprovedLearners (long ukprn, int academicYear)
    {
        var dates = AcademicYearParser.ParseFrom(academicYear);

        var learners = _sqlServerClient.GetList<Http.LearnerDataOuterApiClient.Learning>($"select distinct lrn.[Uln], lrn.[Key] " +
            $" from [dbo].[Learning] lrn " +
            $" inner join [dbo].[Episode] ep on ep.LearningKey = lrn.[Key] " +
            $" inner join [dbo].[EpisodePrice] eppr on eppr.EpisodeKey = ep.[Key] " +
            $" WHERE (eppr.StartDate <= '{dates.End}' AND eppr.EndDate   >= '{dates.Start}') " +
            $" AND ep.Ukprn = {ukprn} And ep.LastDayOfLearning is not null");

        return learners;
    }
}

public class Learning
{
    public Guid Key { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Uln { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string ApprenticeshipHashedId { get; set; } = null!;
    public DateTime? CompletionDate { get; set; } = null;
    public string? EmailAddress { get; set; }
    public List<FreezeRequest> FreezeRequests { get; set; }
    public List<Episode> Episodes { get; set; }
    public List<WithdrawalRequest> WithdrawalRequests { get; set; }
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

public class WithdrawalRequest
{
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public string Reason { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ProviderApprovedBy { get; set; }
}