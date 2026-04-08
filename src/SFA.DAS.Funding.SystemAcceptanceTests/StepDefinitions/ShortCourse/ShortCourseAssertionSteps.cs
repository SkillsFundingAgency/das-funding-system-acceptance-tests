using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Messages.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.ShortCourse;

[Binding]
public class ShortCourseAssertionSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient, LearningSqlClient learningSqlClient, ShortCourseEarningsAndPaymentsAssertionHelper assertionHelper)
{
    [Then(@"remove all earnings for that ""short course""")]
    public async Task ThenRemoveAllEarningsForThatShortCourse()
    {
        await assertionHelper.AssertAllEarningsRemoved();
    }

    [Then(@"remove the remaining completion earning")]
    [Then(@"remove the completion earning")]
    [Then(@"a completion earning is not generated")]
    public async Task ThenRemoveTheRemainingCompletionEarning()
    {
        await assertionHelper.AssertRemainingCompletionEarningRemoved();
    }

    [Then(@"remove the 30% milestone earning")]
    [Then(@"a 30% milestone earning is not generated")]
    public async Task ThenRemoveThe30PercentMilestoneEarning()
    {
        await assertionHelper.Assert30PercentMilestoneEarningRemoved();
    }

    [Then(@"retain the 30% milestone earning")]
    [Then(@"a 30% milestone earning is generated")]
    public async Task ThenRetainThe30PercentMilestoneEarning()
    {
        await assertionHelper.Assert30PercentMilestoneEarningRetained();
    }

    [Then(@"a completion earning is generated")]
    public async Task ThenACompletionEarningIsGenerated()
    {
        await assertionHelper.AssertCompletionEarningGenerated();
    }

    [Given(@"the basic short course earnings are generated")]
    [Then(@"the basic short course earnings are generated")]
    public async Task ThenTheShortCourseIsSuccessfullyProcessed()
    {
        await assertionHelper.AssertBasicShortCourseEarningsGenerated();
    }

    [Then(@"the short course earnings are set to approved")]
    public async Task ThenTheShortCourseEarningsAreSetToApproved()
    {
        await assertionHelper.AssertShortCourseEarningsSetToApproved();
    }

    [Then("the short course earnings do not contain duplicates")]
    public async Task ThenTheShortCourseEarningsAreGeneratedWithoutDuplication()
    {
        await assertionHelper.AssertShortCourseEarningsAreGeneratedWithoutDuplication();
    }

    [Then(@"the learning domain is updated correctly")]
    public async Task ThenTheLearningDbIsUpdatedWithTheShortCourse()
    {
        var testData = context.Get<TestData>();
        var expectedLearner = testData.ShortCourseLearnerData!.Learner;
        var expectedCourse = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();

        ShortCourseLearning? learningModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            learningModel = learningSqlClient.GetShortCourseLearning(testData.Uln.ToString());
            return learningModel != null && learningModel.Episodes != null && learningModel.Episodes.Any();
        }, "Failed to find short course learning entity.");

        context.Set(learningModel);

        var learner = learningModel!.Learner;
        Assert.AreEqual(expectedLearner.FirstName, learner.FirstName, "Learner FirstName does not match.");
        Assert.AreEqual(expectedLearner.LastName, learner.LastName, "Learner LastName does not match.");
        Assert.AreEqual(expectedLearner.Email, learner.EmailAddress, "Learner EmailAddress does not match.");
        Assert.AreEqual(expectedLearner.Dob, learner.DateOfBirth, "Learner DateOfBirth does not match.");

        var episode = learningModel.Episodes.Single();
        Assert.AreEqual(expectedCourse.CourseCode, episode.TrainingCode, "TrainingCode does not match.");
        Assert.AreEqual(Constants.UkPrn, episode.Ukprn, "Ukprn does not match.");
        Assert.AreEqual(expectedCourse.StartDate, episode.StartDate, "StartDate does not match.");
        Assert.AreEqual(expectedCourse.ExpectedEndDate, episode.ExpectedEndDate, "ExpectedEndDate does not match.");
        Assert.AreEqual((byte)LearnerData.Events.LearningType.ApprenticeshipUnit, episode.LearningType, "LearningType does not match.");

        var expectedLearningSupports = expectedCourse.LearningSupport ?? new();
        var actualLearningSupports = episode.LearningSupport ?? new();
        Assert.AreEqual(expectedLearningSupports.Count, actualLearningSupports.Count, "LearningSupport count does not match.");
        
        foreach (var expectedSupport in expectedLearningSupports)
        {
            var actualSupport = actualLearningSupports.FirstOrDefault(x => x.StartDate == expectedSupport.StartDate && x.EndDate == expectedSupport.EndDate);
            Assert.IsNotNull(actualSupport, $"LearningSupport with StartDate {expectedSupport.StartDate} and EndDate {expectedSupport.EndDate} not found.");
        }
    }

    [Then(@"the short course is set to unapproved")]
    public void ThenTheShortCourseIsSetToUnapproved()
    {
        var learningModel = context.Get<ShortCourseLearning>();
        Assert.IsFalse(learningModel.Episodes.Single().IsApproved, "Short course should not be approved.");
    }

    [Then(@"the short course is set to approved")]
    public void ThenTheShortCourseIsSetToApproved()
    {
        var learningModel = context.Get<ShortCourseLearning>();
        Assert.IsTrue(learningModel.Episodes.Single().IsApproved, "Short course should be approved.");
        Assert.AreEqual(context.Get<TestData>().CommitmentsApprenticeshipCreatedEvent.AccountId, learningModel.Episodes.Single().EmployerAccountId, "EmployerId should have been updated from the approvals event.");
    }

    [Then(@"the learner ref is stored in the learning db")]
    public void ThenTheLearnerRefIsStoredInTheLearningDb()
    {
        var testData = context.Get<TestData>();
        var learningModel = context.Get<ShortCourseLearning>();
        var expectedLearnerRef = testData.ShortCourseLearnerData.Learner.LearnerRef;
        var actualLearnerRef = learningModel.Episodes.Single().LearnerRef;

        Assert.AreEqual(expectedLearnerRef, actualLearnerRef, "LearnerRef does not match.");
    }

    [Then(@"the episode keys match between the learning and earnings databases")]
    public async Task ThenTheEpisodeKeysMatchBetweenTheLearningAndEarningsDatabases()
    {
        var testData = context.Get<TestData>();

        var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
        Assert.IsNotNull(earningsModel, "Earnings model not found.");
        var earningsEpisodeKey = earningsModel.Episodes.Single().Key;

        var learningModel = learningSqlClient.GetShortCourseLearning(testData.Uln.ToString());
        Assert.IsNotNull(learningModel, "Learning model not found.");
        var learningEpisodeKey = learningModel.Episodes.Single().Key;

        Assert.AreEqual(learningEpisodeKey, earningsEpisodeKey, "Episode keys do not match between learning and earnings databases.");
    }

    [Then(@"the second instalment is earnt in period (.*)")]
    public async Task ThenTheSecondInstalmentIsEarntInPeriod(TokenisablePeriod period)
    {
        await assertionHelper.AssertSecondInstalmentIsEarntInPeriod(period);
    }

    [When(@"SLD requests short course approved ulns for academic year (.*)")]
    public async Task WhenSldRequestsShortCourseLearnerApprovedUlnsForAcademicYear(TokenisableAcademicYear academicYear)
    {
        var testData = context.Get<TestData>();
        testData.ShortCourseLearnersResponse = await learnerDataOuterApiHelper.GetShortCourseLearnerApprovedUlns(Constants.UkPrn, academicYear.Value);
    }

    [When(@"SLD requests short course earnings data for collection period (.*)")]
    public async Task WhenSldRequestsShortCourseEarningsDataForCollectionPeriod(TokenisablePeriod period)
    {
        var testData = context.Get<TestData>();
        testData.ShortCourseEarningsResponse = await learnerDataOuterApiHelper.GetShortCourseEarningsData(Constants.UkPrn, period.Value.AcademicYear, period.Value.PeriodValue);
    }

    [Then(@"the short course learner is returned in the approved ulns response without duplicates")]
    public void ThenTheShortCourseLearnerIsReturnedInTheApprovedUlnsResponse()
    {
        var testData = context.Get<TestData>();
        
        var learnerCount = testData.ShortCourseLearnersResponse.Learners.Count(x => x.Uln == testData.Uln.ToString());
        Assert.AreEqual(1, learnerCount, "Short course learner was expected exactly once in the approved ulns response for this academic year, but found a different count.");
    }

    [Then(@"the short course learner is not returned in the approved ulns response")]
    public void ThenTheShortCourseLearnerIsNotReturnedInTheApprovedUlnsResponse()
    {
        var testData = context.Get<TestData>();
        
        var learner = testData.ShortCourseLearnersResponse.Learners.SingleOrDefault(x => x.Uln == testData.Uln.ToString());
        Assert.IsNull(learner, "Short course learner was unexpectedly found in the approved ulns response for this academic year.");
    }

    [Then(@"the short course learner is returned in the earnings response without duplicates")]
    public void ThenTheShortCourseLearnerIsReturnedInTheEarningsResponse()
    {
        var testData = context.Get<TestData>();

        var learnerCount = testData.ShortCourseEarningsResponse.Learners.Count(x => x.LearningKey == testData.ShortCourseLearningKey.ToString());
        Assert.AreEqual(1, learnerCount, "Short course learner was expected exactly once in the earnings response for this collection period, but found a different count.");
    }

    [Then(@"the short course learner is not returned in the earnings response")]
    public void ThenTheShortCourseLearnerIsNotReturnedInTheEarningsResponse()
    {
        var testData = context.Get<TestData>();

        var learner = testData.ShortCourseEarningsResponse.Learners.SingleOrDefault(x => x.LearningKey == testData.ShortCourseLearningKey.ToString());
        Assert.IsNull(learner, "Short course learner was unexpectedly found in the earnings response for this collection period.");
    }

    [Then(@"only earnings are generated for the earliest short course")]
    public async Task ThenOnlyEarningsAreGeneratedForTheEarliestShortCourse()
    {
        await assertionHelper.AssertOnlyEarningsAreGeneratedForTheEarliestShortCourse();
    }

    [Then(@"the earnings are still recorded against the first provider")]
    public async Task ThenTheEarningsAreStillRecordedAgainstTheFirstProvider()
    {
        await assertionHelper.AssertEarningsAreStillRecordedAgainstTheFirstProvider();
    }

    [Then(@"the short course data is sent to approvals")]
    public async Task ThenTheShortCourseDataIsSentToApprovals()
    {
        var testData = context.Get<TestData>();
        var shortCourseOnProgramme = testData.ShortCourseLearnerData!.Delivery.OnProgramme.Single();

        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.Count > 0;
        }, "Failed to find short course earnings entity.");

        await WaitHelper.WaitForIt(() =>
        {
            var publishedEvent = LearnerDataEventHandler.GetMessage(x => x.ULN == long.Parse(testData.Uln));
            if (publishedEvent != null)
            {
                Assert.AreEqual(Constants.UkPrn, publishedEvent.UKPRN, "UKPRN does not match");
                Assert.AreEqual(LearnerData.Events.LearningType.ApprenticeshipUnit, publishedEvent.LearningType, "LearningType does not match");
                //Assert.AreEqual(shortCourseOnProgramme.CourseCode, publishedEvent.StandardCode.ToString(), "StandardCode does not match"); TODO assert this correctly when we build 1607, might be called LARSCode on the event
                Assert.AreEqual((int)earningsModel!.Episodes.Single().CoursePrice, publishedEvent.TrainingPrice, "TrainingPrice does not match CoursePrice");
                
                return true;
            }
            return false;
        }, "Failed to find published LearnerDataEvent.");
    }

    [Then(@"(.*) earnings profile history records are created for the short course")]
    public async Task ThenEarningsProfileHistoryRecordsAreCreatedForTheShortCourse(int expectedRecordCount)
    {
        var testData = context.Get<TestData>();
        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
                if (earningsModel == null) return false;

                var episodes = earningsModel.Episodes;
                if (episodes == null || !episodes.Any()) return false;

                var latestEpisode = episodes.OrderByDescending(e => e.StartDate).First();
                if (latestEpisode.EarningsProfile == null) return false;

                return latestEpisode.EarningsProfileHistory != null && latestEpisode.EarningsProfileHistory.Count == expectedRecordCount;
            }, $"Failed to find exactly {expectedRecordCount} history records.");

        var latestEpisode = earningsModel!.Episodes.OrderByDescending(e => e.StartDate).First();
        var historyRecords = latestEpisode.EarningsProfileHistory;

        foreach (var history in historyRecords)
        {
            Assert.AreEqual(latestEpisode.EarningsProfile.EarningsProfileId, history.EarningsProfileId, "EarningsProfileId in history does not match current EarningsProfileId");
            Assert.AreNotEqual(Guid.Empty, history.Version, "Version is empty in history record");
        }
    }
}
