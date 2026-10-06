using System.Globalization;
using GraphQL;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

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

            testData.CalculateGrowthAndSkillsPaymentsEvent =
                growthAndSkillsPayments ?? testData.CalculateGrowthAndSkillsPaymentsEvent;

            return testData.CalculateGrowthAndSkillsPaymentsEvent != null;
        }, "Failed to find growth and skills payments recalculated event.");

        testData.CalculateGrowthAndSkillsPaymentsEvent.Command.Training.LearningType.ToString().Should().Be("Apprenticeship");
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
}
