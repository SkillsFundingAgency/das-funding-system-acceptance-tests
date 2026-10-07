using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using System.Globalization;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship;

[Binding]
public class ApprenticeshipEarningsToPaymentsStepDefinitions(ScenarioContext context, LearningSqlClient learningSqlClient)
{
    [Then("send the apprenticeship approved earnings to payments")]
    public async Task ThenSendTheApprenticeshipApprovedEarningsToPayments(Table table)
    {
        var testData = context.Get<TestData>();

        Guid? learnerKey = null;

        await WaitHelper.WaitForIt(() =>
        {
            var learning = learningSqlClient.TryGetApprenticeshipByUln(testData.Uln);
            learnerKey = learning?.Learner?.Key;
            if (learnerKey == null)
            {
                return false;
            }

            return true;
        }, "Failed to find the expected learner in learning DB");


        await WaitHelper.WaitForIt(() =>
        {
 
            var growthAndSkillsPayments = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == learnerKey);
            testData.CalculateGrowthAndSkillsPaymentsEvent = growthAndSkillsPayments ?? testData.CalculateGrowthAndSkillsPaymentsEvent;

            if (testData.CalculateGrowthAndSkillsPaymentsEvent == null)
            {
                return false;
            }

            var periods = testData.CalculateGrowthAndSkillsPaymentsEvent.Command.Earnings
                .SelectMany(x => x.PricePeriods)
                .SelectMany(x => x.Periods)
                .ToList();

            if (periods.Count != table.Rows.Count)
            {
                return false;
            }

            return table.Rows.All(row =>
            {
                var expectedDeliveryPeriod = int.Parse(GetCellValue(row, "deliveryPeriod"), CultureInfo.InvariantCulture);
                var expectedAmount = decimal.Parse(GetCellValue(row, "amount"), CultureInfo.InvariantCulture);
                var expectedTypes = GetExpectedTypeAliases(GetCellValue(row, "type"));

                return periods.Any(period =>
                    period.DeliveryPeriod == expectedDeliveryPeriod &&
                    expectedTypes.Contains(period.EarningType.ToString(), StringComparer.OrdinalIgnoreCase) &&
                    Convert.ToDecimal(period.Amount, CultureInfo.InvariantCulture) == expectedAmount);
            });
        }, "Failed to find the expected apprenticeship earnings instalments in the growth and skills payments recalculated event.");
    }

    [Then("the apprenticeship \"learning type\" is sent to Payments")]
    public async Task ThenTheApprenticeshipLearningTypeIsSentToPayments()
    {
        var testData = context.Get<TestData>();
        testData.CalculateGrowthAndSkillsPaymentsEvent = await GetGrowthAndSkillsPaymentsEvent(testData);

        testData.CalculateGrowthAndSkillsPaymentsEvent.Command.Training.LearningType.ToString().Should().Be("Apprenticeship");
    }

    [Then("the {word} incentive earning {word} sent to payments for provider & employer")]
    public async Task ThenTheIncentiveEarningIsSentToPaymentsForProviderAndEmployer(string incentiveEarningNumber, string outcome)
    {
        var testData = context.Get<TestData>();
        testData.CalculateGrowthAndSkillsPaymentsEvent = await GetGrowthAndSkillsPaymentsEvent(testData);
        var periods = GetPaymentsPeriods(testData);

        var incentiveExpected = outcome == "is";

        HasIncentivePeriod(periods, incentiveEarningNumber, "provider")
            .Should().Be(incentiveExpected, $"{incentiveEarningNumber} provider incentive payment event should {(incentiveExpected ? "exist" : "not exist")}");

        HasIncentivePeriod(periods, incentiveEarningNumber, "employer")
            .Should().Be(incentiveExpected, $"{incentiveEarningNumber} employer incentive payment event should {(incentiveExpected ? "exist" : "not exist")}");
    }

    [Then("the {word} incentive earning {word} sent to payments for employer")]
    public async Task ThenTheIncentiveEarningIsSentToPaymentsForEmployer(string incentiveEarningNumber, string outcome)
    {
        var testData = context.Get<TestData>();
        testData.CalculateGrowthAndSkillsPaymentsEvent = await GetGrowthAndSkillsPaymentsEvent(testData);
        var periods = GetPaymentsPeriods(testData);

        var incentiveExpected = outcome == "is";

        HasIncentivePeriod(periods, incentiveEarningNumber, "employer")
            .Should().Be(incentiveExpected, $"{incentiveEarningNumber} employer incentive payment event should {(incentiveExpected ? "exist" : "not exist")}");
    }

    [Then("no incentive earning is sent to payments for provider & employer")]
    public async Task ThenNoIncentiveEarningIsSentToPaymentsForProviderAndEmployer()
    {
        var testData = context.Get<TestData>();
        testData.CalculateGrowthAndSkillsPaymentsEvent = await GetGrowthAndSkillsPaymentsEvent(testData);
        var periods = GetPaymentsPeriods(testData);

        periods.Any(x => x.EarningType.ToString().Contains("ProviderIncentive", StringComparison.OrdinalIgnoreCase))
            .Should().BeFalse("no provider incentive payment events should be present");

        periods.Any(x => x.EarningType.ToString().Contains("EmployerIncentive", StringComparison.OrdinalIgnoreCase))
            .Should().BeFalse("no employer incentive payment events should be present");
    }

    private static string GetCellValue(DataTableRow row, params string[] columnNames)
    {
        var matchedKey = row.Keys.FirstOrDefault(key => columnNames.Any(name => key.Equals(name, StringComparison.OrdinalIgnoreCase)));

        if (matchedKey == null)
        {
            throw new KeyNotFoundException($"Expected one of the following columns: {string.Join(", ", columnNames)}");
        }

        return row[matchedKey];
    }

    private static string[] GetExpectedTypeAliases(string type)
    {
        return type.Trim().ToLowerInvariant() switch
        {
            "learning" => ["Learning", "OnProgramme", "OnProgram", "OnProgrammePayment"],
            "completion" => ["Completion"],
            "balancing" => ["Balancing"],
            _ => [type.Trim()]
        };
    }

    private async Task<dynamic> GetGrowthAndSkillsPaymentsEvent(TestData testData)
    {
        Guid? learnerKey = null;

        await WaitHelper.WaitForIt(() =>
        {
            var learning = learningSqlClient.TryGetApprenticeshipByUln(testData.Uln);
            learnerKey = learning?.Learner?.Key;
            return learnerKey != null;
        }, "Failed to find the expected learner in learning DB");

        await WaitHelper.WaitForIt(() =>
        {
            var growthAndSkillsPayments = GrowthAndSkillsPaymentsRecalculatedEventHandler
                .GetMessage(x => x.Command.Learner.LearnerKey == learnerKey);

            testData.CalculateGrowthAndSkillsPaymentsEvent = growthAndSkillsPayments ?? testData.CalculateGrowthAndSkillsPaymentsEvent;

            return testData.CalculateGrowthAndSkillsPaymentsEvent != null;
        }, "Failed to find growth and skills payments recalculated event.");

        return testData.CalculateGrowthAndSkillsPaymentsEvent;
    }

    private static List<dynamic> GetPaymentsPeriods(TestData testData)
    {
        return testData.CalculateGrowthAndSkillsPaymentsEvent.Command.Earnings
            .SelectMany(x => x.PricePeriods)
            .SelectMany(x => x.Periods)
            .Cast<dynamic>()
            .ToList();
    }

    private static bool HasIncentivePeriod(List<dynamic> periods, string incentiveEarningNumber, string recipient)
    {
        var expectedPrefix = incentiveEarningNumber.ToLowerInvariant() switch
        {
            "first" => "First",
            "second" => "Second",
            _ => throw new Exception("Step definition requires 'first' or 'second' to be specified for incentive earning")
        };

        var expectedSuffix = recipient.ToLowerInvariant() switch
        {
            "provider" => "ProviderIncentive",
            "employer" => "EmployerIncentive",
            _ => throw new Exception("Recipient must be provider or employer")
        };

        return periods.Any(x =>
            x.EarningType.ToString().Contains(expectedPrefix, StringComparison.OrdinalIgnoreCase) &&
            x.EarningType.ToString().Contains(expectedSuffix, StringComparison.OrdinalIgnoreCase));
    }
}
