using SFA.DAS.Apprenticeships.Enums;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

public class ApprenticeshipsSqlClient
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

    public Apprenticeship GetApprenticeship(Guid apprenticeshipKey)
    {
        var apprenticeship = _sqlServerClient.GetList<Apprenticeship>("SELECT * FROM [dbo].[Apprenticeship] WHERE [KEY] = @apprenticeshipKey", new {apprenticeshipKey}).FirstOrDefault(); ;
        if (apprenticeship == null)
        {
            throw new InvalidOperationException("No apprenticeship found");
        }
        apprenticeship.Episodes = _sqlServerClient.GetList<Episode>($"SELECT * FROM [dbo].[Episode] WHERE ApprenticeshipKey = '{apprenticeship.Key}'");
        foreach (var episode in apprenticeship.Episodes)
        {
            episode.Prices = _sqlServerClient.GetList<EpisodePrice>($"SELECT * FROM [dbo].[EpisodePrice] WHERE EpisodeKey = '{episode.Key}'");
        }
        apprenticeship.PriceHistories = _sqlServerClient.GetList<PriceHistory>($"SELECT * FROM [dbo].[PriceHistory] WHERE ApprenticeshipKey = '{apprenticeship.Key}'");
        apprenticeship.StartDateChanges = _sqlServerClient.GetList<StartDateChange>($"SELECT * FROM [dbo].[StartDateChange] WHERE ApprenticeshipKey = '{apprenticeship.Key}'");
        apprenticeship.FreezeRequests = _sqlServerClient.GetList<FreezeRequest>($"SELECT * FROM [dbo].[FreezeRequest] WHERE ApprenticeshipKey = '{apprenticeship.Key}'");
        apprenticeship.WithdrawalRequests = _sqlServerClient.GetList<WithdrawalRequest>($"SELECT * FROM [dbo].[WithdrawalRequest] WHERE ApprenticeshipKey = '{apprenticeship.Key}'");
        return apprenticeship;
    }
}

public class Apprenticeship
{
    public Guid Key { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Uln { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
    public string ApprenticeshipHashedId { get; set; } = null!;
    public List<PriceHistory> PriceHistories { get; set; }
    public List<StartDateChange> StartDateChanges { get; set; }
    public List<FreezeRequest> FreezeRequests { get; set; }
    public List<Episode> Episodes { get; set; }
    public List<WithdrawalRequest> WithdrawalRequests { get; set; }
}

public class Episode
{
    public Guid Key { get; set; }
    public Guid ApprenticeshipKey { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public FundingType FundingType { get; set; }
    public FundingPlatform? FundingPlatform { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public long? AccountLegalEntityId { get; set; }
    public string TrainingCode { get; set; } = null!;
    public string? TrainingCourseVersion { get; set; }
    public bool PaymentsFrozen { get; set; }
    public List<EpisodePrice> Prices { get; set; }
    public string LearningStatus { get; set; }

    public DateTime? LastDayOfLearning { get; set; }
}

public class EpisodePrice
{
    public Guid Key { get; set; }
    public Guid EpisodeKey { get; set; }
    public bool IsDeleted { get; set; }
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
    public Guid ApprenticeshipKey { get; set; }
    public string FrozenBy { get; set; } = null!;
    public DateTime FrozenDateTime { get; set; }
    public bool Unfrozen { get; set; }
    public DateTime? UnfrozenDateTime { get; set; }
    public string? UnfrozenBy { get; set; }
    public string? Reason { get; set; }
}

public class PriceHistory
{
    public Guid Key { get; set; }
    public Guid ApprenticeshipKey { get; set; }
    public decimal? TrainingPrice { get; set; }
    public decimal? AssessmentPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime EffectiveFromDate { get; set; }
    public string? ProviderApprovedBy { get; set; }
    public DateTime? ProviderApprovedDate { get; set; }
    public string? EmployerApprovedBy { get; set; }
    public DateTime? EmployerApprovedDate { get; set; }
    public string? ChangeReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public ChangeRequestStatus? PriceChangeRequestStatus { get; set; }
    public string? RejectReason { get; set; }
    public ChangeInitiator? Initiator { get; set; }
}

public class StartDateChange
{
    public Guid Key { get; set; }
    public Guid ApprenticeshipKey { get; set; }
    public DateTime ActualStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Reason { get; set; } = null!;
    public string? ProviderApprovedBy { get; set; }
    public DateTime? ProviderApprovedDate { get; set; }
    public string? EmployerApprovedBy { get; set; }
    public DateTime? EmployerApprovedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public ChangeRequestStatus RequestStatus { get; set; }
    public ChangeInitiator? Initiator { get; set; }
    public string RejectReason { get; set; } = null!;
}

public class WithdrawalRequest
{
    public Guid Key { get; set; }
    public Guid ApprenticeshipKey { get; set; }
    public Guid EpisodeKey { get; set; }
    public string Reason { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ProviderApprovedBy { get; set; }
}