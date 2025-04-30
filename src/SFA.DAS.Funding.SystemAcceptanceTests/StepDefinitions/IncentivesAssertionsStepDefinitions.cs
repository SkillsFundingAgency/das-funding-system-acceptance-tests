using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class IncentivesAssertionsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly PaymentsMessageHandler _paymentsMessageHandler;

    public IncentivesAssertionsStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _paymentsMessageHandler = new PaymentsMessageHandler(context);
    }

    [When(@"the apprentice is marked as a care leaver")]
    public async Task MarkAsCareLeaver()
    {
        var helper = new EarningsInnerApiHelper();
        await helper.MarkAsCareLeaver(_context.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey);
    }


    [Then(@"the (first|second) incentive earning (is|is not) generated for provider & employer")]
    public void VerifyIncentiveEarnings(string incentiveEarningNumber, string outcome)
    {
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>();

        var additionalPayments = _context
            .Get<EarningsApprenticeshipModel>(ContextKeys.InitialEarningsApprenticeshipModel)
            .Episodes
            .SingleOrDefault()
            ?.AdditionalPayments;

        additionalPayments.Should().NotBeNull("No episode found on earnings apprenticeship model");

        var incentiveExpected = outcome == "is";
        var expectation = incentiveExpected ? "Expected" : "Not Expected";

        switch (incentiveEarningNumber)
        {
            case "first":
                additionalPayments!
                    .IncentiveEarningExists(commitmentsApprenticeshipCreatedEvent.StartDate, 1, AdditionalPaymentType.ProviderIncentive)
                    .Should().Be(incentiveExpected, $"First Incentive Earning {expectation} For Provider");
                additionalPayments!
                    .IncentiveEarningExists(commitmentsApprenticeshipCreatedEvent.StartDate, 1, AdditionalPaymentType.EmployerIncentive)
                    .Should().Be(incentiveExpected,$"First Incentive Earning {expectation} For Employer");
                break;
            case "second":
                additionalPayments!
                    .IncentiveEarningExists(commitmentsApprenticeshipCreatedEvent.StartDate, 2, AdditionalPaymentType.ProviderIncentive)
                    .Should().Be(incentiveExpected, $"Second Incentive Earning {expectation} For Provider");
                additionalPayments!
                    .IncentiveEarningExists(commitmentsApprenticeshipCreatedEvent.StartDate, 2, AdditionalPaymentType.EmployerIncentive)
                    .Should().Be(incentiveExpected, $"Second Incentive Earning {expectation} For Employer");
                break;
            default:
                throw new Exception("Step definition requires 'first' or 'second' to be specified for incentive earning");
        }
    }

    [Then(@"the (first|second) incentive payment (is|is not) generated for provider & employer")]
    public async Task VerifyIncentivePayments(string incentiveEarningNumber, string outcome)
    {
        if (incentiveEarningNumber is not ("first" or "second"))
            throw new Exception("This step only supports incentive payments 'first' or 'second'");

        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent>();

        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);
        await _paymentsMessageHandler.ReceivePaymentsEvent(apprenticeshipKey);

        var paymentsGeneratedEvent = _context.Get<PaymentsGeneratedEvent>();
        var paymentsApprenticeshipModel = new PaymentsSqlClient().GetPaymentsModel(_context);

        switch (outcome)
        {
            case "is":
            {
                var expectedPaymentPeriods = new List<PaymentDeliveryPeriodExpectation>
                {
                    PaymentDeliveryPeriodExpectationBuilder.BuildForIncentive(
                        commitmentsApprenticeshipCreatedEvent.StartDate,
                        incentiveEarningNumber == "first" ? (byte)1 : (byte)2,
                        AdditionalPaymentType.ProviderIncentive),
                    PaymentDeliveryPeriodExpectationBuilder.BuildForIncentive(
                        commitmentsApprenticeshipCreatedEvent.StartDate,
                        incentiveEarningNumber == "first" ? (byte)1 : (byte)2,
                        AdditionalPaymentType.EmployerIncentive)
                };

                // Validate PaymentsGenerateEvent & Payments Entity
                expectedPaymentPeriods.AssertAgainstEventPayments(paymentsGeneratedEvent.Payments);

                expectedPaymentPeriods.AssertAgainstEntityArray(paymentsApprenticeshipModel.Payments);
                break;
            }
            case "is not":
                var expectedPeriod = incentiveEarningNumber == "first"
                    ? commitmentsApprenticeshipCreatedEvent.StartDate.AddDays(89).ToAcademicYearAndPeriod() // 90th day of learning
                    : commitmentsApprenticeshipCreatedEvent.StartDate.AddDays(364).ToAcademicYearAndPeriod(); // 365th day of learning

                paymentsGeneratedEvent.Payments.Should().NotContain(x => x.Amount > 0 && x.PaymentType == AdditionalPaymentType.ProviderIncentive.ToString() && x.AcademicYear == expectedPeriod.AcademicYear && x.DeliveryPeriod == expectedPeriod.Period);
                paymentsGeneratedEvent.Payments.Should().NotContain(x => x.Amount > 0 && x.PaymentType == AdditionalPaymentType.EmployerIncentive.ToString() && x.AcademicYear == expectedPeriod.AcademicYear && x.DeliveryPeriod == expectedPeriod.Period);
                paymentsApprenticeshipModel.Payments.Should().NotContain(x => x.Amount > 0 && x.PaymentType == AdditionalPaymentType.ProviderIncentive.ToString() && x.AcademicYear == expectedPeriod.AcademicYear && x.DeliveryPeriod == expectedPeriod.Period);
                paymentsApprenticeshipModel.Payments.Should().NotContain(x => x.Amount > 0 && x.PaymentType == AdditionalPaymentType.EmployerIncentive.ToString() && x.AcademicYear == expectedPeriod.AcademicYear && x.DeliveryPeriod == expectedPeriod.Period);
                break;
            default: throw new Exception("This step only supports and outcome of 'is' or 'is not'");
        }
    }
}