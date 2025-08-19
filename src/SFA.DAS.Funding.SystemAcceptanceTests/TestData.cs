using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Learning.Types;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

internal class TestData
{
    internal string Uln { get; }
    internal Guid LearningKey { get; set; } = Guid.Empty;
    internal Guid InitialEarningsProfileId { get; set; } = Guid.Empty;
    internal Guid EarningsProfileId { get; set; } = Guid.Empty;
    internal bool IsMarkedAsCareLeaver { get; set; } = false;
    internal bool IsLearningSupportAdded { get; set; } = false;
    internal bool IsMathsAndEnglishAdded { get; set; } = false;
    internal string CurrentCollectionYear { get; set; }
    internal byte CurrentCollectionPeriod { get; set; }
    internal decimal NewTrainingPrice { get; set; }
    internal decimal NewAssessmentPrice { get; set; }
    internal decimal NewEarningsAmount { get; set; }
    internal decimal FundingBandMax { get; set; }
    internal DateTime LastDayOfLearning { get; set; }
    internal DateTime PriceChangeEffectiveFrom { get; set; }
    internal DateTime PriceChangeApprovedDate { get; set; }
    internal DateTime StartDateChangeApprovedDate { get; set; }
    internal DateTime NewStartDate { get; set; }
    internal DateTime NewEndDate { get; set; }
    internal DateTime? PlannedEndDate { get; set; }
    internal DateTime? OriginalStartDate { get; set; }
    internal List<TestSupport.Payments> PaymentDbRecords { get; set; } 
    internal List<FM36Learner> FM36Learners { get; set; }
    internal EarningsApprenticeshipModel? EarningsApprenticeshipModel { get; set; }
    internal PaymentsApprenticeshipModel? PaymentsApprenticeshipModel { get; set; }
    internal ApprenticeshipEarningsRecalculatedEvent? ApprenticeshipEarningsRecalculatedEvent { get; set; }
    internal CommitmentsMessages.ApprenticeshipCreatedEvent CommitmentsApprenticeshipCreatedEvent { get; set; }
    internal LearningCreatedEvent LearningCreatedEvent { get; set; }
    internal EarningsGeneratedEvent EarningsGeneratedEvent { get; set; }
    internal List<FinalisedOnProgammeLearningPaymentEvent> FinalisedPaymentsList { get; set; }
    internal List<CalculateOnProgrammePayment> CalculatedOnProgrammePaymentList { get; set; }
    internal LearnerDataOuterApiClient.LearnerDataRequest? LearnerData { get; set; }
    internal LearnerDataBuilder LearnerDataBuilder { get; set; }

    public TestData(string uln)
    {
        Uln = uln;
    }
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.