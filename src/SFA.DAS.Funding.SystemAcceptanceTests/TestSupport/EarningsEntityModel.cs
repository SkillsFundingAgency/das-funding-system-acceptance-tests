using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    
    public class EarningsEntityModel
    {
        public Model? Model { get; set; }
    }

    public class Model
    {
        public Guid ApprenticeshipKey { get; set; }
        public long ApprovalsApprenticeshipId { get; set; }
        public string Uln { get; set; }
        public ApprenticeshipEpisodes[] ApprenticeshipEpisodes { get; set; }
    }

    public class ApprenticeshipEpisodes
    {
        public Guid ApprenticeshipEpisodeKey { get; set; }
        public long UKPRN { get; set; }
        public long EmployerAccountId { get; set; }
        public string LegalEntityName { get; set; }
        public string TrainingCode { get; set; }
        public long? FundingEmployerAccountId { get; set; }
        public FundingType FundingType { get; }
        public int AgeAtStartOfApprenticeship { get; set; }
        public Prices[] Prices { get; set; }
        public EarningsProfile EarningsProfile { get; set; }
        public EarningsProfileHistory[]? EarningsProfileHistory { get; set; }
    }

    public class Prices
    {
        public DateTime ActualStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public double AgreedPrice { get; set; }
        public double FundingBandMaximum { get; set; }
    }
    public class EarningsProfileHistory
    {
        public Record Record { get; set; }
    }

    public class Record
    {
        public Guid EarningsProfileId { get; set; }
        public double AdjustedPrice { get; set; }
        public Instalments[] Instalments { get; set; }
        public double CompletionPayment { get; set; }
    }

    public class EarningsProfile
    {
        public Guid EarningsProfileId { get; set; }
        public double AdjustedPrice { get; set; }
        public Instalments[] Instalments { get; set; }
        public double CompletionPayment { get; set; }
    }

    public class Instalments
    {
        public int AcademicYear { get; set; }
        public int DeliveryPeriod { get; set; }
        public double Amount { get; set; }
    }
}
