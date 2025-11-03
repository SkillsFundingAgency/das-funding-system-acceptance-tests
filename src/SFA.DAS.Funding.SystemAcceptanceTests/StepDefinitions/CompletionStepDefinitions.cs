using Reqnroll;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CompletionStepDefinitions(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient)
    {
        [Given(@"Learning Completion is recorded on (.*)")]
        [When(@"Learning Completion is recorded on (.*)")]
        public async Task WhenSLDInformUsThatTheLearningCompletedOn(TokenisableDateTime completionDate)
        {
            var testData = context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithCompletionDate(completionDate.Value);
        }

        [When("SLD resubmits ILR")]
        public void SLDResubmitsILR()
        {
            var testData = context.Get<TestData>();
            testData.ResetLearnerDataBuilder();
        }


        [When("completion date is removed")]
        public void CompletionDateIsRemoved()
        {
            var testData = context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithCompletionDate(null);
        }


        [Given(@"earnings of (.*) are generated from periods (.*) to (.*)")]
        [When(@"earnings of (.*) are generated from periods (.*) to (.*)")]
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

        [Given(@"an earning of (.*) of type (.*) is generated for period (.*)")]
        [When(@"an earning of (.*) of type (.*) is generated for period (.*)")]
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

        [Then("Balancing earning is removed")]
        public async Task BalancingEarningIsRemoved()
        {
            var earnings = earningsSqlClient.GetEarningsEntityModel(context);
            var episode = earnings.Episodes.SingleOrDefault();

            var instalment = episode.EarningsProfile.Instalments.SingleOrDefault(x => x.Type.Trim() == "Balancing");

            instalment.Should().BeNull();
        }

        [Then("Completion earning is removed")]
        public void CompletionEarningIsRemoved()
        {
            var earnings = earningsSqlClient.GetEarningsEntityModel(context);
            var episode = earnings.Episodes.SingleOrDefault();

            var instalment = episode.EarningsProfile.Instalments.SingleOrDefault(x => x.Type.Trim() == "Completion");

            instalment.Should().BeNull();
        }


    }
}
