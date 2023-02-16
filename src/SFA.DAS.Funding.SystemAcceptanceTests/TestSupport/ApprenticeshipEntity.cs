
namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    
    public class ApprenticeshipEntityModel
    {
        public Model Model { get; set; }
    }
    
    public class Model
    {
        public Guid ApprenticeshipKey { get; set; }
        public long ApprovalsApprenticeshipId { get; set; }
        public string Uln { get;set; }
        public EarningsProfile EarningsProfile { get; set; }
        public double FundingBandMaximum { get; set; }
    }

    public class EarningsProfile
    {
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
