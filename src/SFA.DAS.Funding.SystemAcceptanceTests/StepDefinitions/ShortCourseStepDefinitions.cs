using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class ShortCourseStepDefinitions(ScenarioContext scenarioContext, LearnerDataOuterApiHelper learnerDataOuterApiHelper, LearningSqlClient learningSqlClient, LearnerDataSqlClient learnerDataSqlClient)
    {
        [When(@"SLD inform us of a new Short Course")]
        public async Task WhenSLDInformUsOfANewShortCourse()
        {
            var testData = scenarioContext.Get<TestData>();
            var shortCourseRequestBuilder = testData.GetShortCourseRequestBuilder();

            var shortCourseRequest = shortCourseRequestBuilder.Build();

            await learnerDataOuterApiHelper.PostShortCourse(Constants.UkPrn, shortCourseRequest);
        }

        [Then(@"the Short Course details are recorded in Learning")]
        public void ThenTheShortCourseDetailsAreRecordedInLearning()
        {
            //todo: what to assert?
            var shortCourses = learningSqlClient.GetShortCourseEpisodes(Constants.UkPrn);
            shortCourses.Count.Should().BeGreaterThan(0);
        }

        [Then(@"a LearnerData event is published to approvals")]
        public void ThenALearnerDataEventIsPublishedToApprovals()
        {
            var uln = 123;

            var learnerData = learnerDataSqlClient.GetLearnerData(uln);
            
            //todo: what to assert?
        }

    }
}
