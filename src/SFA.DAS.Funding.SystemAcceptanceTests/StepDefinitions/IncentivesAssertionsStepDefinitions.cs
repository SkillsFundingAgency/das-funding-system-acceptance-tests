using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class IncentivesAssertionsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly EarningsSqlClient _earningsEntitySqlClient;
    private readonly PaymentsSqlClient _paymentsSqlClient;
    private readonly EarningsInnerApiHelper _earningsInnerApiHelper;

    public IncentivesAssertionsStepDefinitions(
        ScenarioContext context, 
        EarningsSqlClient earningsEntitySqlClient, 
        PaymentsSqlClient paymentsSqlClient,
        EarningsInnerApiHelper earningsInnerApiHelper)
    {
        _context = context;
        _earningsEntitySqlClient = earningsEntitySqlClient;
        _paymentsSqlClient = paymentsSqlClient;
        _earningsInnerApiHelper = earningsInnerApiHelper;
    }

    [When(@"the apprentice is marked as a care leaver")]
    public async Task MarkAsCareLeaver()
    {
        var testData = _context.Get<TestData>();
        PaymentsGeneratedEventHandler.Clear(x=> x.ApprenticeshipKey == testData.ApprenticeshipKey);
        await _earningsInnerApiHelper.MarkAsCareLeaver(testData.ApprenticeshipKey);
        testData.IsMarkedAsCareLeaver = true;
    }


    [Then(@"the (first|second) incentive earning (is|is not) generated for provider & employer")]
    public async Task VerifyIncentiveEarnings(string incentiveEarningNumber, string outcome)
    {
        var testData = _context.Get<TestData>();

        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);
            return !testData.IsMarkedAsCareLeaver || earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfileHistory.Any();
        }, "Failed to find updated earnings entity.");

        var additionalPayments = earningsApprenticeshipModel
            .Episodes
            .SingleOrDefault()
            ?.AdditionalPayments;

        testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId;

        additionalPayments.Should().NotBeNull("No episode found on earnings apprenticeship model");

        var incentiveExpected = outcome == "is";
        var expectation = incentiveExpected ? "Expected" : "Not Expected";

        switch (incentiveEarningNumber)
        {
            case "first":
                additionalPayments!
                    .IncentiveEarningExists(testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(), 1, AdditionalPaymentType.ProviderIncentive)
                    .Should().Be(incentiveExpected, $"First Incentive Earning {expectation} For Provider");
                additionalPayments!
                    .IncentiveEarningExists(testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(), 1, AdditionalPaymentType.EmployerIncentive)
                    .Should().Be(incentiveExpected,$"First Incentive Earning {expectation} For Employer");
                break;
            case "second":
                additionalPayments!
                    .IncentiveEarningExists(testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(), 2, AdditionalPaymentType.ProviderIncentive)
                    .Should().Be(incentiveExpected, $"Second Incentive Earning {expectation} For Provider");
                additionalPayments!
                    .IncentiveEarningExists(testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(), 2, AdditionalPaymentType.EmployerIncentive)
                    .Should().Be(incentiveExpected, $"Second Incentive Earning {expectation} For Employer");
                break;
            default:
                throw new Exception("Step definition requires 'first' or 'second' to be specified for incentive earning");
        }
    }

    [Then(@"the (first|second) incentive payment (is|is not) generated for provider & employer")]
    public async Task VerifyIncentivePayments(string incentiveEarningNumber, string outcome)
    {
        var testData = _context.Get<TestData>();

        if (incentiveEarningNumber is not ("first" or "second"))
            throw new Exception("This step only supports incentive payments 'first' or 'second'");

        PaymentsApprenticeshipModel? paymentsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            paymentsApprenticeshipModel = _paymentsSqlClient.GetPaymentsModel(_context);
            return paymentsApprenticeshipModel.Earnings.Any(x => x.EarningsProfileId == testData.EarningsProfileId);
        }, "Failed to find updated payments entity.");

        var paymentsGeneratedEvent = testData.PaymentsGeneratedEvent;
        
        switch (outcome)
        {
            case "is":
            {
                var expectedPaymentPeriods = new List<PaymentDeliveryPeriodExpectation>
                {
                    PaymentDeliveryPeriodExpectationBuilder.BuildForIncentive(
                        testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(),
                        incentiveEarningNumber == "first" ? (byte)1 : (byte)2,
                        AdditionalPaymentType.ProviderIncentive),
                    PaymentDeliveryPeriodExpectationBuilder.BuildForIncentive(
                        testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(),
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
                    ? testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault().AddDays(89).ToAcademicYearAndPeriod() // 90th day of learning
                    : testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault().AddDays(364).ToAcademicYearAndPeriod(); // 365th day of learning

                paymentsGeneratedEvent.Payments.Should().NotContain(x => x.Amount > 0 && x.PaymentType == AdditionalPaymentType.ProviderIncentive.ToString() && x.AcademicYear == expectedPeriod.AcademicYear && x.DeliveryPeriod == expectedPeriod.Period);
                paymentsGeneratedEvent.Payments.Should().NotContain(x => x.Amount > 0 && x.PaymentType == AdditionalPaymentType.EmployerIncentive.ToString() && x.AcademicYear == expectedPeriod.AcademicYear && x.DeliveryPeriod == expectedPeriod.Period);
                paymentsApprenticeshipModel.Payments.Should().NotContain(x => x.Amount > 0 && x.PaymentType == AdditionalPaymentType.ProviderIncentive.ToString() && x.AcademicYear == expectedPeriod.AcademicYear && x.DeliveryPeriod == expectedPeriod.Period);
                paymentsApprenticeshipModel.Payments.Should().NotContain(x => x.Amount > 0 && x.PaymentType == AdditionalPaymentType.EmployerIncentive.ToString() && x.AcademicYear == expectedPeriod.AcademicYear && x.DeliveryPeriod == expectedPeriod.Period);
                break;
            default: throw new Exception("This step only supports and outcome of 'is' or 'is not'");
        }
    }
}