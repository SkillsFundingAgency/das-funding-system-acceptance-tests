using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class LearningSupportAssertionsStepDefinitions(ScenarioContext context, EarningsSqlClient earningsEntitySqlClient)
{
    [When(@"learning support is recorded from (.*) to (.*)")]
    public void AddLearningSupport(TokenisableDateTime learningSupportStart, TokenisableDateTime learningSupportEnd)
    {
        var testData = context.Get<TestData>();

        var learnerDataBuilder = testData.GetLearnerDataBuilder();
        learnerDataBuilder.WithOnProgrammeLearningSupport(learningSupportStart.Value, learningSupportEnd.Value);
        
        testData.IsLearningSupportAdded = true;
    }

    [Then(@"learning support earnings are generated from periods (.*) to (.*)")]
    public async Task VerifyLearningSupportEarnings(TokenisablePeriod learningSupportStart, TokenisablePeriod learningSupportEnd)
    {
        var testData = context.Get<TestData>();
        EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

        await WaitHelper.WaitForIt(() =>
        {
            earningsApprenticeshipModel = earningsEntitySqlClient.GetEarningsEntityModel(context);
            return !testData.IsLearningSupportAdded || earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfileHistory.Any();
        }, "Failed to find updated earnings entity.");

        var additionalPayments = earningsApprenticeshipModel
            .Episodes
            .SingleOrDefault()
            ?.AdditionalPayments;

        testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId;

        additionalPayments.Should().NotBeNull("No episode found on earnings apprenticeship model");

        additionalPayments.Should().NotContain(x =>
                new Period(x.AcademicYear, x.DeliveryPeriod).IsBefore(learningSupportStart.Value),
            $"Expected no Learning Support earnings before {learningSupportStart.Value.ToCollectionPeriodString()}");

        additionalPayments.Should().NotContain(x =>
                learningSupportEnd.Value.IsBefore(new Period(x.AcademicYear, x.DeliveryPeriod)),
            $"Expected no Learning Support earnings after {learningSupportEnd.Value.ToCollectionPeriodString()}");

        while (learningSupportStart.Value.IsBefore(learningSupportEnd.Value))
        {
            additionalPayments.Should().ContainSingle(x =>
                    x.AdditionalPaymentType == AdditionalPaymentType.LearningSupport
                    && x.Amount == 150
                    && x.AcademicYear == learningSupportStart.Value.AcademicYear
                    && x.DeliveryPeriod == learningSupportStart.Value.PeriodValue, $"Expected learning support earning for {learningSupportStart.Value.ToCollectionPeriodString()}");

            learningSupportStart.Value = learningSupportStart.Value.GetNextPeriod();
        }
    }
}