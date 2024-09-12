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

    public EarningsEntityModel? GetEarningsEntityModel(ScenarioContext context)
    {
        var earningsEvent = context.Get<EarningsGeneratedEvent>();
        var apprenticeshipKey = earningsEvent.ApprenticeshipKey;

        var apprenticeship = _sqlServerClient.GetList<Model>($"SELECT [Key] as ApprenticeshipKey, ApprovalsApprenticeshipId, Uln FROM [Domain].[Apprenticeship] Where [key] ='{apprenticeshipKey}'").Single();
        var apprenticeshipEpisodes = _sqlServerClient.GetList<ApprenticeshipEpisodes>($"SELECT [Key] as ApprenticeshipEpisodeKey, Ukprn, EmployerAccountId, LegalEntityName, TrainingCode, FundingEmployerAccountId, FundingType, AgeAtStartOfApprenticeship FROM [Domain].[Episode] Where ApprenticeshipKey ='{apprenticeshipKey}'");

        foreach(var episode in apprenticeshipEpisodes)
        {
            episode.Prices = _sqlServerClient.GetList<PriceModel>($"SELECT [Key] as PriceKey, StartDate as ActualStartDate, EndDate as PlannedEndDate, AgreedPrice, FundingBandMaximum FROM [Domain].[EpisodePrice] Where EpisodeKey ='{episode.ApprenticeshipEpisodeKey}'");
            episode.EarningsProfile = _sqlServerClient.GetList<EarningsProfileEntityModel>($"SELECT EarningsProfileId, OnProgramTotal as AdjustedPrice, CompletionPayment FROM [Domain].[EarningsProfile] Where EpisodeKey ='{episode.ApprenticeshipEpisodeKey}'").Single();
            episode.EarningsProfile.Instalments = _sqlServerClient.GetList<InstalmentEntityModel>($"SELECT Amount, AcademicYear, DeliveryPeriod FROM [Domain].[Instalment] Where EarningsProfileId ='{episode.EarningsProfile.EarningsProfileId}'");

            var historyRecords = _sqlServerClient.GetList<EarningsProfileHistoryRecord>($"SELECT EarningsProfileId, OnProgramTotal as AdjustedPrice, CompletionPayment, SupersededDate FROM [Domain].[EarningsProfileHistory] Where EpisodeKey ='{episode.ApprenticeshipEpisodeKey}'");
            episode.EarningsProfileHistory = historyRecords.Select(r => new HistoryRecord<EarningsProfileEntityModel> { 
                Record = new EarningsProfileEntityModel { 
                    AdjustedPrice = r.AdjustedPrice, 
                    CompletionPayment = r.CompletionPayment, 
                    EarningsProfileId = r.EarningsProfileId,
                    Instalments = _sqlServerClient.GetList<InstalmentEntityModel>($"SELECT Amount, AcademicYear, DeliveryPeriod FROM [Domain].[InstalmentHistory] Where EarningsProfileId ='{r.EarningsProfileId}'")
                }, SupersededDate = r.SupersededDate 
            }).ToList();

        }

        apprenticeship.ApprenticeshipEpisodes = apprenticeshipEpisodes;

        return new EarningsEntityModel
        {
            Model = apprenticeship
        };
    }

    public class EarningsProfileHistoryRecord : EarningsProfileEntityModel
    {
        public DateTime SupersededDate { get; set; }
    }
}
