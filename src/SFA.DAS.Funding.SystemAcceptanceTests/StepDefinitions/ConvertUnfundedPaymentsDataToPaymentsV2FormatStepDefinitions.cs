using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Payments.FundingSource.Messages.Commands;
using SFA.DAS.Payments.Model.Core.OnProgramme;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions
{
    private readonly ScenarioContext _context;

    public ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions(ScenarioContext context)
    {
        _context = context;
    }

    [Then(@"(.*) Calculated Required Levy Amount event is published with required values")]
    public async Task CalculatedRequiredLevyAmountEventIsPublishedWithRequiredValues(int numberOfEvents)
    {
        var testData = _context.Get<TestData>();
        var finalisedPaymentsList =
            FinalisedOnProgrammeLearningPaymentEventHandler.ReceivedEvents.Where(x => x.Message.ApprenticeshipKey == testData.ApprenticeshipKey).Select(x => x.Message).ToList();

        var orderedFinalisedPaymentsList = finalisedPaymentsList
            .OrderBy(x => x.ApprenticeshipEarning.DeliveryPeriod)
            .ToList();

        await _context.ReceiveCalculateOnProgrammePaymentEvent(testData.Uln, numberOfEvents);

        var calculatedRequiredLevyAmountList = testData.CalculatedOnProgrammePaymentList
            .OrderBy(x => x.DeliveryPeriod)
            .ToList();

        Assert.That(orderedFinalisedPaymentsList.Count, Is.EqualTo(calculatedRequiredLevyAmountList.Count),
            "The count for FinalisedOnProgrammeLearningPaymentEvent does not match with CalculateOnProgrammePayment.");


        for (int i = 0; i < calculatedRequiredLevyAmountList.Count; i++)
        {
            Assert.Multiple(() =>
            {
                Assert.That(calculatedRequiredLevyAmountList[i].AgreedOnDate, Is.Null, "Incorrect Agreed On Date found");
                Assert.That(calculatedRequiredLevyAmountList[i].OnProgrammeEarningType, Is.EqualTo(OnProgrammeEarningType.Learning), "Incorrect OnProgramme Earning Type found");
                Assert.That(calculatedRequiredLevyAmountList[i].PriceEpisodeIdentifier, Is.Empty, "Incorrect Price Episode Identifier found");
                Assert.That(calculatedRequiredLevyAmountList[i].ActualEndDate, Is.Null, "Incorrect Actual End Date found");
                Assert.That(calculatedRequiredLevyAmountList[i].CompletionStatus, Is.EqualTo(1), "Incorrect Completion Status found");
                Assert.That(calculatedRequiredLevyAmountList[i].CompletionAmount, Is.Zero, "Incorrect Completion Amount found");
                Assert.That(calculatedRequiredLevyAmountList[i].ApprenticeshipPriceEpisodeId, Is.Null, "Incorrect Apprenticeship Price Episode Id");
                Assert.That(calculatedRequiredLevyAmountList[i].EventTime.DateTime, Is.GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-10)), "Incorrect Event Time found");
                Assert.That(calculatedRequiredLevyAmountList[i].EventId, Is.Not.Null.And.TypeOf<Guid>(), "Incorrect Event Id type found");
                Assert.That(calculatedRequiredLevyAmountList[i].LearningAim.Reference, Is.EqualTo("ZPROG001"), "Incorrect - Learning Aim - Reference found");
                Assert.That(calculatedRequiredLevyAmountList[i].LearningAim.ProgrammeType, Is.EqualTo(25), "Incorrect Learning Aim - Programme Type found");
                Assert.That(calculatedRequiredLevyAmountList[i].LearningAim.FrameworkCode, Is.Zero, "Incorrect Learning Aim - Framework code found");
                Assert.That(calculatedRequiredLevyAmountList[i].AmountDue, Is.EqualTo(orderedFinalisedPaymentsList[i].Amount), "Incorrect Amount Due found");
                Assert.That(calculatedRequiredLevyAmountList[i].DeliveryPeriod, Is.EqualTo(orderedFinalisedPaymentsList[i].ApprenticeshipEarning.DeliveryPeriod), "Incorrect Delivery Period found");
                Assert.That(calculatedRequiredLevyAmountList[i].StartDate, Is.EqualTo(orderedFinalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Start Date found");
                Assert.That(calculatedRequiredLevyAmountList[i].PlannedEndDate, Is.EqualTo(orderedFinalisedPaymentsList[i].ApprenticeshipEarning.PlannedEndDate), "Incorrect Planned End Date found");
                Assert.That(calculatedRequiredLevyAmountList[i].LearningStartDate, Is.EqualTo(orderedFinalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Learnings Start Date found");
                Assert.That(calculatedRequiredLevyAmountList[i].Learner.Uln, Is.EqualTo(orderedFinalisedPaymentsList[i].ApprenticeshipEarning.Uln), "Incorrect ULN found");
                Assert.That(calculatedRequiredLevyAmountList[i].LearningAim.FundingLineType, Is.EqualTo(testData.EarningsGeneratedEvent.DeliveryPeriods[0].FundingLineType), "Incorrect Learning Aim - Funding Line Type found");
                Assert.That(calculatedRequiredLevyAmountList[i].LearningAim.StartDate, Is.EqualTo(orderedFinalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Learning Aim - Start Date found");
                Assert.That(calculatedRequiredLevyAmountList[i].CollectionPeriod.AcademicYear, Is.EqualTo(orderedFinalisedPaymentsList[i].CollectionYear), "Incorrect Collection Period - Acadmic Year found");
                Assert.That(calculatedRequiredLevyAmountList[i].CollectionPeriod.Period, Is.EqualTo(orderedFinalisedPaymentsList[i].CollectionPeriod), "Incorrect Collection Period - Period found");
                Assert.That(calculatedRequiredLevyAmountList[i].SfaContributionPercentage, Is.EqualTo(CalculateGovernmentContributionPercentage(testData.EarningsGeneratedEvent)), "Incorrect Sfa Contribution Percentage found");
                Assert.That(calculatedRequiredLevyAmountList[i].TransferSenderAccountId, Is.EqualTo(testData.CommitmentsApprenticeshipCreatedEvent.AccountId), "Incorrect Transfer Sender Account Id found");
                Assert.That(calculatedRequiredLevyAmountList[i].InstalmentAmount, Is.EqualTo(orderedFinalisedPaymentsList[i].Amount), "Incorrect Instalment Amount found");
                Assert.That(calculatedRequiredLevyAmountList[i].NumberOfInstalments, Is.EqualTo(orderedFinalisedPaymentsList[i].ApprenticeshipEarning.NumberOfInstalments), "Incorrect Number of Installments found");
                Assert.That(calculatedRequiredLevyAmountList[i].ApprenticeshipId, Is.EqualTo(testData.CommitmentsApprenticeshipCreatedEvent.ApprenticeshipId), "Incorrect Apprenticeship Id found");
                Assert.That(calculatedRequiredLevyAmountList[i].ApprenticeshipEmployerType.ToString(), Is.EqualTo(testData.CommitmentsApprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval.ToString()), "Incorrect Apprenticeship Employer Type found");
                Assert.That(calculatedRequiredLevyAmountList[i].LearningAim.StandardCode, Is.EqualTo(Convert.ToInt16(testData.CommitmentsApprenticeshipCreatedEvent.TrainingCode)), "Incorrect Learning Aim - Standard Code found");
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
