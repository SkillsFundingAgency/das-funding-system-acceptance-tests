using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using CMT = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions
{
    private readonly ScenarioContext _context;
    private List<FinalisedOnProgammeLearningPaymentEvent> _finalisedPaymentsList;
    private List<CalculateOnProgrammePayment> _calculatedRequiredLevyAmountList;

    public ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    [Then(@"(.*) Calculated Required Levy Amount event is published with required values")]
    public async Task CalculatedRequiredLevyAmountEventIsPublishedWithRequiredValues(int numberOfEvents)
    {
        var testData = _context.Get<TestData>();
        var finalisedPaymentsList =
            FinalisedOnProgrammeLearningPaymentEventHandler.ReceivedEvents.Where(x => x.message.ApprenticeshipKey == testData.ApprenticeshipKey).Select(x => x.message).ToList();

        _finalisedPaymentsList = finalisedPaymentsList
            .OrderBy(x => x.ApprenticeshipEarning.DeliveryPeriod)
            .ToList();

        await _context.ReceiveCalculateOnProgrammePaymentEvent(testData.ApprenticeshipCreatedEvent.Uln, numberOfEvents);

        _calculatedRequiredLevyAmountList = _context.Get<List<CalculateOnProgrammePayment>>()
            .OrderBy(x => x.DeliveryPeriod)
            .ToList();

        Assert.That(_finalisedPaymentsList.Count, Is.EqualTo(_calculatedRequiredLevyAmountList.Count),
            "The count for FinalisedOnProgrammeLearningPaymentEvent does not match with CalculateOnProgrammePayment.");


        for (int i = 0; i < _calculatedRequiredLevyAmountList.Count; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(_calculatedRequiredLevyAmountList[i].AgreedOnDate, Is.Null, "Incorrect Agreed On Date found");
                Assert.That(_calculatedRequiredLevyAmountList[i].OnProgrammeEarningType, Is.EqualTo(OnProgrammeEarningType.Learning), "Incorrect OnProgramme Earning Type found");
                Assert.That(_calculatedRequiredLevyAmountList[i].PriceEpisodeIdentifier, Is.Empty, "Incorrect Price Episode Identifier found");
                Assert.That(_calculatedRequiredLevyAmountList[i].ActualEndDate, Is.Null, "Incorrect Actual End Date found");
                Assert.That(_calculatedRequiredLevyAmountList[i].CompletionStatus, Is.EqualTo(1), "Incorrect Completion Status found");
                Assert.That(_calculatedRequiredLevyAmountList[i].CompletionAmount, Is.Zero, "Incorrect Completion Amount found");
                Assert.That(_calculatedRequiredLevyAmountList[i].ApprenticeshipPriceEpisodeId, Is.Null, "Incorrect Apprenticeship Price Episode Id");
                Assert.That(_calculatedRequiredLevyAmountList[i].EventTime.DateTime, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-10)), "Incorrect Event Time found");
                Assert.That(_calculatedRequiredLevyAmountList[i].EventId, Is.Not.Null.And.TypeOf<Guid>(), "Incorrect Event Id type found");
                Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.Reference, Is.EqualTo("ZPROG001"), "Incorrect - Learning Aim - Reference found");
                Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.ProgrammeType, Is.EqualTo(25), "Incorrect Learning Aim - Programme Type found");
                Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.FrameworkCode, Is.Zero, "Incorrect Learning Aim - Framework code found");
                Assert.That(_calculatedRequiredLevyAmountList[i].AmountDue, Is.EqualTo(_finalisedPaymentsList[i].Amount), "Incorrect Amount Due found");
                Assert.That(_calculatedRequiredLevyAmountList[i].DeliveryPeriod, Is.EqualTo(_finalisedPaymentsList[i].ApprenticeshipEarning.DeliveryPeriod), "Incorrect Delivery Period found");
                Assert.That(_calculatedRequiredLevyAmountList[i].StartDate, Is.EqualTo(_finalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Start Date found");
                Assert.That(_calculatedRequiredLevyAmountList[i].PlannedEndDate, Is.EqualTo(_finalisedPaymentsList[i].ApprenticeshipEarning.PlannedEndDate), "Incorrect Planned End Date found");
                Assert.That(_calculatedRequiredLevyAmountList[i].LearningStartDate, Is.EqualTo(_finalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Learnings Start Date found");
                Assert.That(_calculatedRequiredLevyAmountList[i].Learner.Uln, Is.EqualTo(_finalisedPaymentsList[i].ApprenticeshipEarning.Uln), "Incorrect ULN found");
                Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.FundingLineType, Is.EqualTo(testData.EarningsGeneratedEvent.DeliveryPeriods[0].FundingLineType), "Incorrect Learning Aim - Funding Line Type found");
                Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.StartDate, Is.EqualTo(_finalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Learning Aim - Start Date found");
                Assert.That(_calculatedRequiredLevyAmountList[i].CollectionPeriod.AcademicYear, Is.EqualTo(_finalisedPaymentsList[i].CollectionYear), "Incorrect Collection Period - Acadmic Year found");
                Assert.That(_calculatedRequiredLevyAmountList[i].CollectionPeriod.Period, Is.EqualTo(_finalisedPaymentsList[i].CollectionPeriod), "Incorrect Collection Period - Period found");
                Assert.That(_calculatedRequiredLevyAmountList[i].SfaContributionPercentage, Is.EqualTo(CalculateGovernmentContributionPercentage(testData.EarningsGeneratedEvent)), "Incorrect Sfa Contribution Percentage found");
                Assert.That(_calculatedRequiredLevyAmountList[i].TransferSenderAccountId, Is.EqualTo(testData.CommitmentsApprenticeshipCreatedEvent.AccountId), "Incorrect Transfer Sender Account Id found");
                Assert.That(_calculatedRequiredLevyAmountList[i].InstalmentAmount, Is.EqualTo(_finalisedPaymentsList[i].Amount), "Incorrect Instalment Amount found");
                Assert.That(_calculatedRequiredLevyAmountList[i].NumberOfInstalments, Is.EqualTo(_finalisedPaymentsList[i].ApprenticeshipEarning.NumberOfInstalments), "Incorrect Number of Installments found");
                Assert.That(_calculatedRequiredLevyAmountList[i].ApprenticeshipId, Is.EqualTo(testData.CommitmentsApprenticeshipCreatedEvent.ApprenticeshipId), "Incorrect Apprenticeship Id found");
                Assert.That(_calculatedRequiredLevyAmountList[i].ApprenticeshipEmployerType.ToString(), Is.EqualTo(testData.CommitmentsApprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval.ToString()), "Incorrect Apprenticeship Employer Type found");
                Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.StandardCode, Is.EqualTo(Convert.ToInt16(testData.CommitmentsApprenticeshipCreatedEvent.TrainingCode)), "Incorrect Learning Aim - Standard Code found");
            });
        }
    }

    private decimal CalculateGovernmentContributionPercentage(EarningsGeneratedEvent earningsEvent)
    {
        if (earningsEvent.EmployerType == EmployerType.NonLevy && earningsEvent.AgeAtStartOfApprenticeship < 22)
            return 1;

        return 0.95m;
    }
}
