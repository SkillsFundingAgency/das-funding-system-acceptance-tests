using SFA.DAS.Apprenticeships.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;


public class EarningsApprenticeshipModel // In the earnings repo this is called ApprenticeshipModel, but to avoid confusion with other tests Its prefixed with Earnings
{
    public Guid Key { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Uln { get; set; }
    public List<EpisodeModel> Episodes { get; set; } = null;
}

public class EpisodeModel
{
    public Guid Key { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public FundingType FundingType { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public string TrainingCode { get; set; } = null!;
    public int AgeAtStartOfApprenticeship { get; set; }
    public List<EpisodePriceModel> Prices { get; set; }
    public EarningsProfileModel EarningsProfile { get; set; }
    public List<EarningsProfileHistoryModel> EarningsProfileHistory { get; set; }
    public List<AdditionalPaymentsModel> AdditionalPayments { get; set; }
}

public class EpisodePriceModel
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal AgreedPrice { get; set; }
    public decimal FundingBandMaximum { get; set; }
}

public abstract class EarningsProfileModelBase
{
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public decimal OnProgramTotal { get; set; }
    public decimal CompletionPayment { get; set; }
}

public class EarningsProfileModel : EarningsProfileModelBase
{
    public List<InstalmentModel> Instalments { get; set; } = null!;
}

public class EarningsProfileHistoryModel : EarningsProfileModelBase
{

    public List<InstalmentHistoryModel> Instalments { get; set; } = null!;
    public DateTime SupersededDate { get; set; }
}

public abstract class InstalmentModelBase
{
    public Guid Key { get; set; }
    public Guid EarningsProfileId { get; set; }
    public short AcademicYear { get; set; }
    public byte DeliveryPeriod { get; set; }
    public decimal Amount { get; set; }
}

public class InstalmentModel : InstalmentModelBase
{

}

public class InstalmentHistoryModel : InstalmentModelBase
{

}

public class AdditionalPaymentsModel : InstalmentModelBase
{
    public AdditionalPaymentType AdditionalPaymentType { get; set; }
    public DateTime DueDate { get; set; }
}

public enum AdditionalPaymentType
{
    ProviderIncentive,
    EmployerIncentive
}