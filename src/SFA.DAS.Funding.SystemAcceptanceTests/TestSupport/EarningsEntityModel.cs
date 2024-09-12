using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;


public class EarningsEntityModel
{
    public Model? Model { get; set; }
}

public class Model
{
    public Guid ApprenticeshipKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Uln { get; set; }
    public List<ApprenticeshipEpisodes> ApprenticeshipEpisodes { get; set; } = null;
}

public class ApprenticeshipEpisodes
{
    public Guid ApprenticeshipEpisodeKey { get; set; }
    public long UKPRN { get; set; }
    public long EmployerAccountId { get; set; }
    public string LegalEntityName { get; set; } = null;
    public string TrainingCode { get; set; } = null;
    public long? FundingEmployerAccountId { get; set; }
    public FundingType FundingType { get; }
    public int AgeAtStartOfApprenticeship { get; set; }
    public List<PriceModel> Prices { get; set; }
    public EarningsProfileEntityModel EarningsProfile { get; set; }
    public List<HistoryRecord<EarningsProfileEntityModel>> EarningsProfileHistory { get; set; }
}

public class PriceModel
{
    public Guid PriceKey { get; set; }
    public DateTime ActualStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public decimal AgreedPrice { get; set; }
    public decimal FundingBandMaximum { get; set; }
}

public class HistoryRecord<T> where T : class
{
    public T Record { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}

public class EarningsProfileEntityModel
{
    public Guid EarningsProfileId { get; set; }
    public decimal AdjustedPrice { get; set; }
    public List<InstalmentEntityModel> Instalments { get; set; }
    public decimal CompletionPayment { get; set; }
}

public class InstalmentEntityModel
{
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
}
