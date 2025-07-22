using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.LearnerDataSqlClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class LearnerDataSteps(ScenarioContext context)
    {
        private readonly LearnerDataOuterApiHelper _learnerDataOuterApiHelper = new();
        private readonly LearnerDataSqlClient _learnerDataSqlClient = new();

        [When(@"SLD inform us of a new Learner")]
        public async Task WhenSldInformUsOfANewLearner()
        {
            var testData = context.Get<TestData>();
            var learnerData = await _learnerDataOuterApiHelper.AddLearnerData(testData.Uln, 10005077, 2425);
            testData.LearnerData = learnerData;
            context.Set(testData);
        }

        [Then(@"the learner is added to LearnerData")]
        public async Task ThenTheLearnerIsAddedToLearnerData()
        {
            var testData = context.Get<TestData>();
            var uln = testData.Uln;

            await WaitHelper.WaitForIt(() => _learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln)) != null, "Unable to find LearnerData for Uln");

            var data = _learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln));

            Assert.IsNotNull(data);

            data.Should().BeEquivalentTo(testData.LearnerData, options => options
                .ExcludingMissingMembers()
                .WithMapping<LearnerData>(src => src.LearnerEmail, dest => dest.Email)
                .WithMapping<LearnerData>(src => src.DateOfBirth, dest => dest.DoB)
                .Using<DateTime>(ctx => ctx.Subject.Date.Should().Be(ctx.Expectation.Date))
                .WhenTypeIs<DateTime>()
            );
        }
    }
}
