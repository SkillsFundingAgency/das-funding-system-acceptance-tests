using SFA.DAS.Payments.Messages.Core.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;

namespace SFA.DAS.Payments.FundingSource.Messages.Commands;//Note: This is the namespace that the CalculateOnProgrammePayment class is in and intentionally does not match the project

public class CalculateOnProgrammePayment
{
    public long Ukprn { get; set; }

    public DateTime? AgreedOnDate { get; set; }

    public decimal SfaContributionPercentage { get; set; }

    public OnProgrammeEarningType OnProgrammeEarningType { get; set; }

    public string PriceEpisodeIdentifier { get; set; }

    public decimal AmountDue { get; set; }

    public byte DeliveryPeriod { get; set; }

    public long? AccountId { get; set; }

    public long? TransferSenderAccountId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime PlannedEndDate { get; set; }

    public DateTime? ActualEndDate { get; set; }

    public byte CompletionStatus { get; set; }

    public decimal CompletionAmount { get; set; }

    public decimal InstalmentAmount { get; set; }

    public short NumberOfInstalments { get; set; }

    public DateTime? LearningStartDate { get; set; }

    public long? ApprenticeshipId { get; set; }

    public long? ApprenticeshipPriceEpisodeId { get; set; }

    public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }

    public DateTimeOffset EventTime { get; set; }

    public Guid EventId { get; set; }

    public Learner Learner { get; set; }

    public LearningAim LearningAim { get; set; }

    public CollectionPeriod CollectionPeriod { get; set; }

    public FundingPlatformType FundingPlatformType { get; set; }
}

public enum FundingPlatformType : byte
{
    SubmitLearnerData = 1,
    DigitalApprenticeshipService
}