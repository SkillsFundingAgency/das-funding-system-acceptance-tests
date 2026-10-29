using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using System.Globalization;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship;

[Binding]
public class ApprenticeshipEarningsToPaymentsStepDefinitions(ScenarioContext context, LearningSqlClient learningSqlClient)
{
    [Then("the {word} earnings event is sent to payments")]
    public async Task ThenSendTheApprovedEarningsToPayments(string learningType)
    {
        var testData = context.Get<TestData>();

        var normalizedLearningType = NormalizeLearningType(learningType);
        var growthAndSkillsPaymentsEvent = await GetGrowthAndSkillsPaymentsEvent(testData, normalizedLearningType);

        switch (normalizedLearningType)
        {
            case CourseType.Apprenticeship:
                testData.ApprenticeshipGrowthAndSkillsPaymentsEvent = growthAndSkillsPaymentsEvent;
                break;
            case CourseType.FunctionalSkill:
                testData.MathsAndEnglishGrowthAndSkillsPaymentsEvent = growthAndSkillsPaymentsEvent;
                break;
            case CourseType.ShortCourse:
                testData.ShortCourseGrowthAndSkillsPaymentsEvent = growthAndSkillsPaymentsEvent;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(learningType), learningType,
                    "Supported values are apprenticeship, shortcourse and functionalskill");
        }
    }

    [Then("send the (.*) approved earnings to payments")]
    public async Task ThenSendTheApprovedEarningsToPayments(string learningType, Table table)
    {
        await ThenSendTheApprovedEarningsToPayments(learningType);

        var testData = context.Get<TestData>();
        var growthAndSkillsPaymentsEvent = GetCapturedGrowthAndSkillsPaymentsEvent(learningType, testData);

        var periods = GetPaymentsPeriods(growthAndSkillsPaymentsEvent);

        periods.Count.Should().Be(table.Rows.Count,
            "expected number of payments periods should match expected rows");

        table.Rows.All(row =>
        {
            var expectedDeliveryPeriod = int.Parse(GetCellValue(row, "deliveryPeriod"), CultureInfo.InvariantCulture);
            var expectedAmount = decimal.Parse(GetCellValue(row, "amount"), CultureInfo.InvariantCulture);
            var expectedTypes = GetExpectedTypeAliases(GetCellValue(row, "type"));

            return periods.Any(period =>
                period.DeliveryPeriod == expectedDeliveryPeriod &&
                expectedTypes.Contains(period.EarningType, StringComparer.OrdinalIgnoreCase) &&
                period.Amount == expectedAmount);
        }).Should().BeTrue("expected payments periods should match expected values");
    }

    [Then("the {word} incentive earning {word} sent to payments for provider & employer")]
    public async Task ThenTheIncentiveEarningIsSentToPaymentsForProviderAndEmployer(string incentiveEarningNumber, string outcome)
    {
        var testData = context.Get<TestData>();
        var growthAndSkillsPaymentsEvent = testData.ApprenticeshipGrowthAndSkillsPaymentsEvent;

        var periods = GetPaymentsPeriods(growthAndSkillsPaymentsEvent);

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
        var growthAndSkillsPaymentsEvent = testData.ApprenticeshipGrowthAndSkillsPaymentsEvent;

        var periods = GetPaymentsPeriods(growthAndSkillsPaymentsEvent);

        var incentiveExpected = outcome == "is";

        HasIncentivePeriod(periods, incentiveEarningNumber, "employer")
            .Should().Be(incentiveExpected, $"{incentiveEarningNumber} employer incentive payment event should {(incentiveExpected ? "exist" : "not exist")}");
    }

    [Then("no incentive earning is sent to payments for provider & employer")]
    public async Task ThenNoIncentiveEarningIsSentToPaymentsForProviderAndEmployer()
    {
        var testData = context.Get<TestData>();
        var growthAndSkillsPaymentsEvent = testData.ApprenticeshipGrowthAndSkillsPaymentsEvent;

        var periods = GetPaymentsPeriods(growthAndSkillsPaymentsEvent);

        periods.Any(x => x.EarningType.ToString().Contains("ProviderIncentive", StringComparison.OrdinalIgnoreCase))
            .Should().BeFalse("no provider incentive payment events should be present");

        periods.Any(x => x.EarningType.ToString().Contains("EmployerIncentive", StringComparison.OrdinalIgnoreCase))
            .Should().BeFalse("no employer incentive payment events should be present");
    }

    [Then("(.*) learning support earnings are sent to payments from periods (.*) to (.*)")]
    public async Task ThenLearningSupportEarningsAreSentToPaymentsFromPeriods(string learningType, TokenisablePeriod learningSupportStart, TokenisablePeriod learningSupportEnd)
    {
        var testData = context.Get<TestData>();
        var growthAndSkillsPaymentsEvent = GetCapturedGrowthAndSkillsPaymentsEvent(learningType, testData);

        var periods = GetPaymentsPeriods(growthAndSkillsPaymentsEvent);

        var learningSupportPeriods = periods
            .Where(x => x.EarningType.Contains("LearningSupport", StringComparison.OrdinalIgnoreCase))
            .Select(x => new Period(
                x.AcademicYear,
                x.DeliveryPeriod))
            .ToList();

        learningSupportPeriods.Should().NotBeEmpty("expected learning support earnings to be sent to payments");

        learningSupportPeriods.Should().NotContain(x =>
                x.IsBefore(learningSupportStart.Value),
            $"Expected no Learning Support payment earnings before {learningSupportStart.Value.ToCollectionPeriodString()}");

        learningSupportPeriods.Should().NotContain(x =>
                learningSupportEnd.Value.IsBefore(x),
            $"Expected no Learning Support payment earnings after {learningSupportEnd.Value.ToCollectionPeriodString()}");

        while (learningSupportStart.Value.IsBefore(learningSupportEnd.Value.GetNextPeriod()))
        {
            learningSupportPeriods.Should().ContainSingle(x =>
                x.AcademicYear == learningSupportStart.Value.AcademicYear &&
                x.PeriodValue == learningSupportStart.Value.PeriodValue,
                $"Expected a single Learning Support payment earning for {learningSupportStart.Value.ToCollectionPeriodString()}");

            learningSupportStart.Value = learningSupportStart.Value.GetNextPeriod();
        }
    }

    [Then("no {word} learning support earnings are sent to payments")]
    public async Task ThenNoLearningSupportApprovedEarningsAreSentToPayments(string learningType)
    {
        var testData = context.Get<TestData>();
        var growthAndSkillsPaymentsEvent = GetCapturedGrowthAndSkillsPaymentsEvent(learningType, testData);

        var periods = GetPaymentsPeriods(growthAndSkillsPaymentsEvent);

        periods.Any(x => x.EarningType.Contains("LearningSupport", StringComparison.OrdinalIgnoreCase))
            .Should().BeFalse("no learning support payment earnings should be present");
    }

    [Then("the {word} earnings to payments event has no instalments")]
    public async Task ThenNoEarningsAreSentToPayments(string learningType)
    {
        var testData = context.Get<TestData>();
        var growthAndSkillsPaymentsEvent = GetCapturedGrowthAndSkillsPaymentsEvent(learningType, testData);

        var periods = GetPaymentsPeriods(growthAndSkillsPaymentsEvent);

        periods.Should().BeEmpty("expected no apprenticeship earnings periods to be sent to payments");
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


    private async Task<GrowthAndSkillsPaymentsRecalculatedEvent> GetGrowthAndSkillsPaymentsEvent(
        TestData testData,
        CourseType courseType)
    {
        Guid? learnerKey = null;

        await WaitHelper.WaitForIt(() =>
        {
            var learning = learningSqlClient.TryGetApprenticeshipByUln(testData.Uln);
            learnerKey = learning?.Learner?.Key;
            return learnerKey != null;
        }, "Failed to find the expected learner in learning DB");

        GrowthAndSkillsPaymentsRecalculatedEvent? growthAndSkillsPaymentsEvent = null;
        await WaitHelper.WaitForIt(() =>
        {
            growthAndSkillsPaymentsEvent = GrowthAndSkillsPaymentsRecalculatedEventHandler
                .GetMessage(x => x.Command.Learner.LearnerKey == learnerKey && 
                                x.Command.Training.CourseType == courseType);

            return growthAndSkillsPaymentsEvent != null;
        }, $"Failed to find growth and skills payments recalculated event for learner key {learnerKey} and course type {courseType}");

        return growthAndSkillsPaymentsEvent!;
    }

    private static GrowthAndSkillsPaymentsRecalculatedEvent GetCapturedGrowthAndSkillsPaymentsEvent(string? learningType, TestData testData)
    {
        var normalizedLearningType = NormalizeLearningType(learningType);

        return normalizedLearningType switch
        {
            CourseType.Apprenticeship => testData.ApprenticeshipGrowthAndSkillsPaymentsEvent
                                ?? throw new InvalidOperationException("Apprenticeship payments event has not been captured. Call 'the apprenticeship earnings are sent to payments' first."),
            CourseType.FunctionalSkill => testData.MathsAndEnglishGrowthAndSkillsPaymentsEvent
                                 ?? throw new InvalidOperationException("Functional skill payments event has not been captured. Call 'the functionalSkill earnings are sent to payments' first."),
            CourseType.ShortCourse => testData.ShortCourseGrowthAndSkillsPaymentsEvent
                              ?? throw new InvalidOperationException("Short course payments event has not been captured. Call 'the shortCourse earnings are sent to payments'  first."),
            _ => throw new InvalidOperationException("Approved earnings learning type has not been set. Call the approved earnings capture step first.")
        };
    }

    private static CourseType NormalizeLearningType(string? learningType)
    {
        var normalized = learningType?.Replace(" ", string.Empty).ToLowerInvariant();
        return normalized switch
        {
            "apprenticeship" => CourseType.Apprenticeship,
            "functionalskill" => CourseType.FunctionalSkill,
            "shortcourse" => CourseType.ShortCourse,
            _ => throw new ArgumentOutOfRangeException(nameof(learningType), learningType,
                "Supported values are apprenticeship, shortcourse and functionalskill")
        };
    }

    private static List<PaymentsPeriodModel> GetPaymentsPeriods(GrowthAndSkillsPaymentsRecalculatedEvent growthAndSkillsPaymentsEvent)
    {
        return growthAndSkillsPaymentsEvent.Command.Earnings
            .SelectMany(earning => earning.PricePeriods
                .SelectMany(pricePeriod => pricePeriod.Periods
                    .Select(period => new PaymentsPeriodModel(
                        Convert.ToInt16(earning.AcademicYear, CultureInfo.InvariantCulture),
                        Convert.ToByte(period.DeliveryPeriod, CultureInfo.InvariantCulture),
                        Convert.ToDecimal(period.Amount, CultureInfo.InvariantCulture),
                        period.EarningType.ToString()))))
            .ToList();
    }

    private static bool HasIncentivePeriod(List<PaymentsPeriodModel> periods, string incentiveEarningNumber, string recipient)
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
            x.EarningType.Contains(expectedPrefix, StringComparison.OrdinalIgnoreCase) &&
            x.EarningType.Contains(expectedSuffix, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record PaymentsPeriodModel(short AcademicYear, byte DeliveryPeriod, decimal Amount, string EarningType);
}
