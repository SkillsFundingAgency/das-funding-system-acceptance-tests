using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CompletionStepDefinitions(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient)
    {
        [When(@"SLD inform us that the Learning Completed on (.*)")]
        public async Task WhenSLDInformUsThatTheLearningCompletedOn(TokenisableDateTime completionDate)
        {
            var testData = context.Get<TestData>();
            await learnerDataOuterApiHelper.CompleteLearning(testData.LearningKey, completionDate.Value);
        }

        [When("SLD inform us of learning support request from (.*) to (.*)")]
        public async Task SLDInformUsOfLearningSupportRequest(TokenisableDateTime learningSupportStartDate, TokenisableDateTime learningSupportEndDate)
        {
            var testData = context.Get<TestData>();

            await learnerDataOuterApiHelper.AddOnProgrammeLearningSupport(testData.LearningKey, learningSupportStartDate.Value, learningSupportEndDate.Value);
        }


        [Then(@"earnings of (.*) are generated from periods (.*) to (.*)")]
        public async Task ThenEarningsOfAreGeneratedBetweenPeriods(decimal amount, TokenisablePeriod periodFrom, TokenisablePeriod periodTo)
        {
            var earnings = earningsSqlClient.GetEarningsEntityModel(context);

            var episode = earnings.Episodes.SingleOrDefault();

            var regularInstalments = episode.EarningsProfile.Instalments.Where(x => x.Type.Trim() == "Regular").ToList();

            regularInstalments.Should()
                .NotContain(x => new Period(x.AcademicYear, x.DeliveryPeriod).IsBefore(periodFrom.Value));

            regularInstalments.Should()
                .Contain(x => new Period(x.AcademicYear, x.DeliveryPeriod).IsBefore(periodTo.Value));

            while (periodFrom.Value.IsBefore(periodTo.Value))
            {
                regularInstalments.Should().Contain(x =>
                        x.Amount == amount
                        && x.AcademicYear == periodFrom.Value.AcademicYear
                        && x.DeliveryPeriod == periodFrom.Value.PeriodValue,
                    $"Expected regular instalment of {amount} for {periodFrom.Value.ToCollectionPeriodString()}");

                periodFrom.Value = periodFrom.Value.GetNextPeriod();
            }
        }

        [Then(@"an earning of (.*) of type (.*) is generated for period (.*)")]
        public async Task ThenABalancingEarningOfIsGeneratedForPeriod(decimal amount, string earningType, TokenisablePeriod period)
        {
            var earnings = earningsSqlClient.GetEarningsEntityModel(context);
            var episode = earnings.Episodes.SingleOrDefault();

            var instalment = episode.EarningsProfile.Instalments.SingleOrDefault(x => x.Type.Trim() == earningType);
            instalment.Should().NotBeNull();

            var deliveryPeriod = new Period(instalment.AcademicYear, instalment.DeliveryPeriod);
            deliveryPeriod.AcademicYear.Should().Be(period.Value.AcademicYear);
            deliveryPeriod.PeriodValue.Should().Be(period.Value.PeriodValue);

            instalment.Amount.Should().Be(amount);
       }
    }
}
