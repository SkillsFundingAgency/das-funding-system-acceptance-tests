using Microsoft.Azure.Amqp.Framing;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class IncentivesAssertionsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly EarningsSqlClient _earningsEntitySqlClient;
    private readonly EarningsInnerApiHelper _earningsInnerApiHelper;

    public IncentivesAssertionsStepDefinitions(
        ScenarioContext context,
        EarningsSqlClient earningsEntitySqlClient,
        EarningsInnerApiHelper earningsInnerApiHelper)
    {
        _context = context;
        _earningsEntitySqlClient = earningsEntitySqlClient;
        _earningsInnerApiHelper = earningsInnerApiHelper;
    }

    [When(@"the apprentice is marked as a care leaver")]
    public async Task MarkAsCareLeaver()
    {
        var testData = _context.Get<TestData>();
        await _earningsInnerApiHelper.MarkAsCareLeaver(testData.LearningKey);
        testData.IsMarkedAsCareLeaver = true;
    }

    [Given(@"the (first|second) incentive earning (is|is not) generated for provider & employer")]
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
                    .Should().Be(incentiveExpected, $"First Incentive Earning {expectation} For Employer");
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

        additionalPayments!.Count(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive).Should().BeLessThanOrEqualTo(2, "No more than two employer incentive payments should be made.");
        additionalPayments!.Count(x => x.AdditionalPaymentType == AdditionalPaymentType.ProviderIncentive).Should().BeLessThanOrEqualTo(2, "No more than two provider incentive payments should be made.");
    }

    [Then("no incentive earning is generated for provider & employer")]
    public async Task NoIncentiveEarningIsGenerated()
    {
        var testData = _context.Get<TestData>();

        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);
            return !testData.IsMarkedAsCareLeaver || earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId == testData.EarningsProfileId;
        }, "Failed to find earnings entity.");

        var additionalPayments = earningsApprenticeshipModel
            .Episodes
            .SingleOrDefault()
            ?.AdditionalPayments;

        additionalPayments.Should().NotContain(x => x.AdditionalPaymentType == AdditionalPaymentType.ProviderIncentive);
        additionalPayments.Should().NotContain(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive);
    }

    [Given(@"the (first|second) incentive due date for provider & employer is (.*)")]
    [Then(@"the (first|second) incentive due date for provider & employer is (.*)")]
    public async Task VerifyIncentiveEarningsDueDate(string incentiveEarningNumber, DateTime dueDate)
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

        additionalPayments.Should().NotBeNull("No episode found on earnings apprenticeship model");

        var breaksInLearning = earningsApprenticeshipModel
            .Episodes.
            SingleOrDefault()
            ?.EpisodeBreakInLearning;

        switch (incentiveEarningNumber)
        {
            case "first":
                additionalPayments!
                    .IncentiveEarningExists(testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(), 1, AdditionalPaymentType.ProviderIncentive, dueDate, breaksInLearning)
                    .Should().Be(true, $"Incorrect First Incentive Earning For Provider");
                additionalPayments!
                    .IncentiveEarningExists(testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(), 1, AdditionalPaymentType.EmployerIncentive, dueDate, breaksInLearning)
                    .Should().Be(true, $"Incorrect First Incentive Earning For Provider");
                break;
            case "second":
                additionalPayments!
                    .IncentiveEarningExists(testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(), 2, AdditionalPaymentType.ProviderIncentive, dueDate, breaksInLearning)
                    .Should().Be(true, $"Incorrect Second Incentive Earning For Provider");
                additionalPayments!
                    .IncentiveEarningExists(testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault(), 2, AdditionalPaymentType.EmployerIncentive, dueDate, breaksInLearning)
                    .Should().Be(true, $"Incorrect Second Incentive Earning For Provider");
                break;
            default:
                throw new Exception("Step definition requires 'first' or 'second' to be specified for incentive earning");
        }
    }
}