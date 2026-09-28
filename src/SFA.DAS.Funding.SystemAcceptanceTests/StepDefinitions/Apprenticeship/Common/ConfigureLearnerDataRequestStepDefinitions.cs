using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship.Common;

[Binding]
public class ConfigureLearnerDataRequestStepDefinitions(ScenarioContext context)
{
    [Given("SLD record on-programme cost as total price (.*) from date (.*) to date (.*)")]
    [When("SLD record on-programme cost as total price (.*) from date (.*) to date (.*)")]
    public void SLDRecordOnProgrammeCostFromDate(int totalPrice, TokenisableDateTime fromDate, TokenisableDateTime toDate)
    {
        var testData = context.Get<TestData>();

        var trainingPrice = totalPrice * 0.8;
        var epaoPrice = totalPrice * 0.2;

        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithCostDetails((int)trainingPrice, (int)epaoPrice, fromDate.Value);

        learnerDataBuilder.WithExpectedEndDate(toDate.Value);

        learnerDataBuilder.WithStandardCode(Convert.ToInt32(testData.CommitmentsApprenticeshipCreatedEvent.TrainingCode));
    }

    [Given("SLD record on-programme cost as total price (.*) from date (.*) with duration (.*)")]
    [When("SLD record on-programme cost as total price (.*) from date (.*) with duration (.*)")]
    public void SLDRecordOnProgrammeCostFromDateWithDuration(int totalPrice, TokenisableDateTime fromDate, int duration)
    {
        var testData = context.Get<TestData>();

        var plannedEndDate = fromDate.Value.AddDays(duration - 1);

        var trainingPrice = totalPrice * 0.8;
        var epaoPrice = totalPrice * 0.2;

        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithCostDetails((int)trainingPrice, (int)epaoPrice, fromDate.Value);

        learnerDataBuilder.WithExpectedEndDate(plannedEndDate);
    }

    [When("SLD record on-programme training price (.*) with epao as (.*) from date (.*) to date (.*)")]
    public void SLDRecordOnProgrammeTrainingPriceAndEpaoFromDate(string trainingPrice, string epaoPrice, string fromDate, TokenisableDateTime toDate)
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        int? epao = string.IsNullOrWhiteSpace(epaoPrice) || epaoPrice.Equals("null", StringComparison.OrdinalIgnoreCase)
            ? null
            : Convert.ToInt32(epaoPrice);

        int? tp = string.IsNullOrWhiteSpace(trainingPrice) || trainingPrice.Equals("null", StringComparison.OrdinalIgnoreCase)
            ? null
            : Convert.ToInt32(trainingPrice);

        TokenisableDateTime? fd = string.IsNullOrWhiteSpace(fromDate) || fromDate.Equals("null", StringComparison.OrdinalIgnoreCase)
            ? null
            : TokenisableDateTime.FromString(fromDate);

        learnerDataBuilder.WithCostDetails(tp, epao, fd == null ? null : fd.Value);

        learnerDataBuilder.WithExpectedEndDate(toDate.Value);

        learnerDataBuilder.WithStandardCode(Convert.ToInt32(testData.CommitmentsApprenticeshipCreatedEvent.TrainingCode));
    }

    [When("SLD record an empty on-programme costs array")]
    public void SLDRecordEmptyOnProgCostsArray()
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithEmptyCostDetails();
    }

    [Given("SLD record expected end date (.*)")]
    [When("SLD record expected end date (.*)")]
    public void SLDRecordExpectedEndDate(TokenisableDateTime plannedEndDate)
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithExpectedEndDate(plannedEndDate.Value);
    }

    [When("SLD record standard code as (.*)")]
    public void WhenSLDRecordStandardCodeAs(int standardCode)
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithStandardCode(standardCode);
    }

    [When("SLD record on-prog start date as (.*)")]
    public void WhenSLDRecordOn_ProgStartDateAsPreviousAY(TokenisableDateTime startDate)
    {
        var testData = context.Get<TestData>();
        var learnerDataBuilder = testData.GetLearnerDataBuilder();

        learnerDataBuilder.WithStartDate(startDate.Value);
    }

    [When("SLD record latest on-programme cost as total price (.*)")]
    public void SLDRecordOnProgrammeCostFromDate(int totalPrice)
    {
        var testData = context.Get<TestData>();

        var trainingPrice = totalPrice * 0.8;
        var epaoPrice = totalPrice * 0.2;

        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithLatestPeriodOfLearningHavingCost((int)trainingPrice, (int)epaoPrice);
    }
}
