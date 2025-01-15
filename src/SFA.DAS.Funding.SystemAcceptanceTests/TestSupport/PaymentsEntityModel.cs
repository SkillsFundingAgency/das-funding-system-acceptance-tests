
namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

#pragma warning disable CS8618

public class PaymentsApprenticeshipModel
{
    public Guid ApprenticeshipKey { get; set; }
    public long FundingEmployerAccountId { get; set; }
    public string EmployerType { get; set; }
    public long TransferSenderAccountId { get; set; }
    public long Uln { get; set; }
    public long Ukprn { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string CourseCode { get; set; }
    public DateTime StartDate { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public bool PaymentsFrozen { get; set; }
    public int AgeAtStartOfApprenticeship { get; set; }
    public string LearnerReference { get; set; }

    public List<Earnings> Earnings { get; set;}
    public List<Payments> Payments { get; set; }
}

public class Earnings
{
    public Guid Key { get; set; }
    public Guid ApprenticeshipKey { get; set; }
    public Guid EarningsProfileId { get; set; }
    public int DeliveryPeriod { get; set; }
    public int AcademicYear { get; set; }
    public int CollectionMonth { get; set; }
    public int CollectionYear { get; set; }
    public double Amount { get; set; }
    public string FundingLineType { get; set; }
}

public class Payments
{
    public Guid Key { get; set; }
    public Guid ApprenticeshipKey { get; set; }
    public Guid EarningsProfileId { get; set; }
    public int AcademicYear { get; set; }
    public int DeliveryPeriod { get; set; }
    public double Amount { get; set; }
    public int CollectionYear { get; set; }
    public int CollectionPeriod { get; set; }
    public bool SentForPayment { get; set; }
    public string FundingLineType { get; set; }
    public bool NotPaidDueToFreeze { get; set; }
}

#pragma warning restore CS8618