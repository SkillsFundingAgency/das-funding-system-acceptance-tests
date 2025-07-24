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

        [Then(@"earnings of (.*) are generated from periods (.*) to (.*)")]
        public async Task ThenEarningsOfAreGeneratedBetweenPeriods(decimal amount, TokenisablePeriod periodFrom, TokenisablePeriod periodTo)
        {
            var earnings = earningsSqlClient.GetEarningsEntityModel(context);

            var episode = earnings.Episodes.SingleOrDefault();

            var regularInstalments = episode.EarningsProfile.Instalments.Where(x => x.Type == "Regular").ToList();

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

        [Then(@"a (.*) earning of (.*) is generated for period (.*)")]
        public async Task ThenABalancingEarningOfIsGeneratedForPeriod(string earningType, decimal amount, TokenisablePeriod period)
        {
            var earnings = earningsSqlClient.GetEarningsEntityModel(context);
            var episode = earnings.Episodes.SingleOrDefault();

            var instalment = episode.EarningsProfile.Instalments.SingleOrDefault(x => x.Type == earningType);
            instalment.Should().NotBeNull();

            var deliveryPeriod = new Period(instalment.AcademicYear, instalment.DeliveryPeriod);
            deliveryPeriod.Should().Be(period.Value);

            instalment.Amount.Should().Be(amount);
       }
    }
}
