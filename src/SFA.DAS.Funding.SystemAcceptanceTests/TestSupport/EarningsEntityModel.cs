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
        public DateTime ActualStartDate { get; set; }
        public DateTime PlannedEndDate {  get; set; }
        public EarningsProfile EarningsProfile { get; set; }
        public EarningsProfileHistory[]? EarningsProfileHistory { get; set; } 
        public double FundingBandMaximum { get; set; }
        public double AgreedPrice { get; set; }
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
