using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class ShortCourseStepDefinitions(ScenarioContext scenarioContext, LearnerDataOuterApiHelper learnerDataOuterApiHelper, LearningSqlClient learningSqlClient, LearnerDataSqlClient learnerDataSqlClient)
    {
        [When(@"SLD record a new Short Course with a start date of (.*) and an expected end date of (.*)")]
        public async Task WhenSLDInformUsOfANewShortCourseWithAStartDateOfStart_DateAndAnExpectedEndDateOfEnd_Date(TokenisableDateTime start, TokenisableDateTime end)
        {
            var testData = scenarioContext.Get<TestData>();
            var shortCourseRequestBuilder = testData.GetShortCourseRequestBuilder();

            shortCourseRequestBuilder
                .WithStartDate(start.Value)
                .WithExpectedEndDate(end.Value);
        }

        [When(@"SLD submit the Short Course details")]
        public async Task WhenSLDSubmitTheShortCourseDetails()
        {
            var testData = scenarioContext.Get<TestData>();
            var shortCourseRequestBuilder = testData.GetShortCourseRequestBuilder();
            var shortCourseRequest = shortCourseRequestBuilder.Build();
            await learnerDataOuterApiHelper.PostShortCourse(Constants.UkPrn, shortCourseRequest);
        }

        [Then(@"the Short Course details are recorded in Learning")]
        public void ThenTheShortCourseDetailsAreRecordedInLearning()
        {
            var testData = scenarioContext.Get<TestData>();
            var request = testData.GetShortCourseRequestBuilder().Build();

            var shortCourseEpisodes = learningSqlClient.GetShortCourseEpisodes(Constants.UkPrn, testData.Uln);

            shortCourseEpisodes.Count.Should().BeGreaterThan(0);

            var firstEpisode = shortCourseEpisodes.First();

            firstEpisode.StartDate.Date.Should().Be(request.Delivery.OnProgramme.First().StartDate);
            firstEpisode.ExpectedEndDate.Date.Should().Be(request.Delivery.OnProgramme.First().ExpectedEndDate);
        }

        //[Then(@"a LearnerData event is published to approvals")]
        //public void ThenALearnerDataEventIsPublishedToApprovals()
        //{
        //    var testData = scenarioContext.Get<TestData>();
        //    var request = testData.GetShortCourseRequestBuilder().Build();

        //    var uln = long.Parse(testData.Uln);

        //    var learnerData = learnerDataSqlClient.GetLearnerData(uln);

        //    learnerData.StartDate.Should().Be(request.Delivery.OnProgramme.First().StartDate);
        //    learnerData.PlannedEndDate.Should().Be(request.Delivery.OnProgramme.First().ExpectedEndDate);
        //}
    }
}
