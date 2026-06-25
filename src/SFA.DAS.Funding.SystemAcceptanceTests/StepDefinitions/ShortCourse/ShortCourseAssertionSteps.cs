using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Payments.EarningEvents.Messages.External;

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

    [Then(@"the 30% milestone earning is (.*) and the completion earning is (.*)")]
    public async Task ThenTheMilestoneEarningAndCompletionEarningAre(string milestoneStatus, string completionStatus)
    {
        await assertionHelper.AssertMilestoneAndCompletionEarningsStatus(milestoneStatus, completionStatus);
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
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];
        var expectedLearner = shortCourseRequest.Learner;
        var expectedCourse = shortCourseRequest.Delivery.OnProgramme.Single();

        var courseCode = expectedCourse.CourseCode;

        List<ShortCourseLearning>? learningModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            learningModel = learningSqlClient.GetShortCourseLearning(testData.Uln.ToString());
            // ensure the returned model and its Episodes/learner are available
            return learningModel != null && learningModel.FirstOrDefault()?.Learner != null && (learningModel.FirstOrDefault()?.Episodes != null && learningModel.Count > 0);
        }, "Failed to find short course learning entity.");

        context.Set(learningModel);

        var learner = learningModel!.FirstOrDefault()?.Learner;
        Assert.AreEqual(expectedLearner.FirstName, learner.FirstName, "Learner FirstName does not match.");
        Assert.AreEqual(expectedLearner.LastName, learner.LastName, "Learner LastName does not match.");
        Assert.AreEqual(expectedLearner.Email, learner.EmailAddress, "Learner EmailAddress does not match.");
        Assert.AreEqual(expectedLearner.Dob, learner.DateOfBirth, "Learner DateOfBirth does not match.");

        var episode = learningModel.GetEpisode(ukprn, courseCode);
        Assert.AreEqual(expectedCourse.CourseCode, episode.TrainingCode, "TrainingCode does not match.");
        Assert.AreEqual(Constants.UkPrn, episode.Ukprn, "Ukprn does not match.");
        Assert.AreEqual(expectedCourse.StartDate, episode.StartDate, "StartDate does not match.");
        Assert.AreEqual(expectedCourse.ExpectedEndDate, episode.ExpectedEndDate, "ExpectedEndDate does not match.");
        Assert.AreEqual((byte)LearnerData.Events.LearningType.ApprenticeshipUnit, episode.LearningType, "LearningType does not match.");

        if(testData.IsShortCourseApproved && testData.CommitmentsApprenticeshipCreatedEvent?.ApprenticeshipEmployerTypeOnApproval != null)
            Assert.AreEqual((byte)testData.CommitmentsApprenticeshipCreatedEvent.ApprenticeshipEmployerTypeOnApproval, episode.EmployerType, "EmployerType does not match.");

        if (episode.IsApproved)
        {
            Assert.AreEqual(testData.CommitmentsApprenticeshipCreatedEvent?.AccountId, episode.EmployerAccountId, "Employer AccountId do not match.");
            Assert.AreEqual(testData.CommitmentsApprenticeshipCreatedEvent?.TransferSenderId, episode.TransferSenderId, "TransferSenderId do not match.");
            Assert.AreEqual(testData.CommitmentsApprenticeshipCreatedEvent?.ApprenticeshipId, episode.ApprovalsApprenticeshipId, "Approvals Apprenticeship Id do not match.");
        }
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
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];
        var courseCode = shortCourseRequest.Delivery.OnProgramme.Single().CourseCode;
        var learningModel = context.Get<List<ShortCourseLearning>>();
        Assert.IsFalse(learningModel.GetEpisode(ukprn, courseCode).IsApproved, "Short course should not be approved.");
    }

    [Then(@"the short course is set to approved")]
    public void ThenTheShortCourseIsSetToApproved()
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];
        var courseCode = shortCourseRequest.Delivery.OnProgramme.Single().CourseCode;
        var learningModel = context.Get<List<ShortCourseLearning>>();
        var learningEpisode = learningModel.GetEpisode(ukprn, courseCode);
        Assert.IsTrue(learningEpisode.IsApproved, "Short course should be approved.");
        Assert.AreEqual(context.Get<TestData>().CommitmentsApprenticeshipCreatedEvent.AccountId, learningEpisode.EmployerAccountId, "EmployerId should have been updated from the approvals event.");
    }

    [Then(@"the learner ref is stored in the learning db")]
    public void ThenTheLearnerRefIsStoredInTheLearningDb()
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];
        var courseCode = shortCourseRequest.Delivery.OnProgramme.Single().CourseCode;
        var learningModel = context.Get<List<ShortCourseLearning>>();
        var expectedLearnerRef = shortCourseRequest.Learner.LearnerRef;
        var actualLearnerRef = learningModel.GetEpisode(ukprn, courseCode).LearnerRef;

        Assert.AreEqual(expectedLearnerRef, actualLearnerRef, "LearnerRef does not match.");
    }

    [Then(@"the episode keys match between the learning and earnings databases")]
    public async Task ThenTheEpisodeKeysMatchBetweenTheLearningAndEarningsDatabases()
    {
        var testData = context.Get<TestData>();

        var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
        Assert.IsNotNull(earningsModel, "Earnings model not found.");
        var earningsEpisodeKey = earningsModel.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent).Key;

        var learningModel = learningSqlClient.GetShortCourseLearning(testData.Uln.ToString());
        Assert.IsNotNull(learningModel, "Learning model not found.");
        var learningEpisodeKey = learningModel.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent).Key;

        Assert.AreEqual(learningEpisodeKey, earningsEpisodeKey, "Episode keys do not match between learning and earnings databases.");
    }

    [Then(@"the second instalment is earnt in period (.*)")]
    public async Task ThenTheSecondInstalmentIsEarntInPeriod(TokenisablePeriod period)
    {
        await assertionHelper.AssertSecondInstalmentIsEarntInPeriod(period);
    }

    [When(@"SLD requests short course approved ulns for academic year (.*)")]
    [Then(@"SLD requests short course approved ulns for academic year (.*)")]
    public async Task WhenSldRequestsShortCourseLearnerApprovedUlnsForAcademicYear(TokenisableAcademicYear academicYear)
    {
        var testData = context.Get<TestData>();
        testData.ShortCourseLearnersResponse = await learnerDataOuterApiHelper.GetShortCourseLearnerApprovedUlns(Constants.UkPrn, academicYear.Value);
    }

    [When("SLD requests short course approved ulns for Provider (.*) in academic year (.*)")]
    [Then("SLD requests short course approved ulns for Provider (.*) in academic year (.*)")]
    public async Task WhenSLDRequestsShortCourseApprovedUlnsForProviderAInAcademicYearCurrentAY(string provider, TokenisableAcademicYear academicYear)
    {
        var testData = context.Get<TestData>();

        long ukprn = UkprnProvider.GetUkprnForProvider(provider);
        testData.ShortCourseLearnersResponse = await learnerDataOuterApiHelper.GetShortCourseLearnerApprovedUlns(ukprn, academicYear.Value);
    }


    [When(@"SLD requests short course earnings data for collection period (.*)")]
    public async Task WhenSldRequestsShortCourseEarningsDataForCollectionPeriod(TokenisablePeriod period)
    {
        var testData = context.Get<TestData>();
        testData.ShortCourseEarningsResponse = await learnerDataOuterApiHelper.GetShortCourseEarningsData(Constants.UkPrn, period.Value.AcademicYear, period.Value.PeriodValue);
    }

    [When(@"SLD requests short course earnings data for provider (.*) and collection period (.*)")]
    public async Task SldRequestsShortCourseEarningsDataForProviderAndCollectionPeriod(string provider, TokenisablePeriod period)
    {
        var testData = context.Get<TestData>();

        long ukprn = UkprnProvider.GetUkprnForProvider(provider);
        testData.ShortCourseEarningsResponse = await learnerDataOuterApiHelper.GetShortCourseEarningsData(ukprn, period.Value.AcademicYear, period.Value.PeriodValue);
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

        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[Constants.UkPrn];
        var courseCode = shortCourseRequest.Delivery.OnProgramme.Single().CourseCode;

        var shortCourseLearningKey = learningSqlClient.GetShortCourseLearning(testData.Uln).GetEpisode(Constants.UkPrn, courseCode).LearningKey;

        var learnerCount = testData.ShortCourseEarningsResponse.Learners.Count(x => x.LearningKey == shortCourseLearningKey.ToString());
        Assert.AreEqual(1, learnerCount, "Short course learner was expected exactly once in the earnings response for this collection period, but found a different count.");
    }

    [Then(@"the short course learner is returned as (.*) in the earnings response")]
    public void ShortCourseLearnerIsReturnedInTheEarningsResponse(string action)
    {
        if (action != "approved" && action != "unapproved" )
            throw new Exception ($"Invalid action - {action}");

        var testData = context.Get<TestData>();

        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[Constants.UkPrn];
        var courseCode = shortCourseRequest.Delivery.OnProgramme.Single().CourseCode;

        var shortCourseLearningKey = learningSqlClient.GetShortCourseLearning(testData.Uln)?.GetEpisode(Constants.UkPrn, courseCode).LearningKey;

        var learner = testData.ShortCourseEarningsResponse?.Learners.Where(x => x.LearningKey == shortCourseLearningKey.ToString()).FirstOrDefault();

        if (action == "approved")
            Assert.IsTrue(learner?.Courses.FirstOrDefault()?.Approved, "Short Course earnings are not set as approved!");
        else 
            Assert.IsFalse(learner?.Courses.FirstOrDefault()?.Approved, "Short Course earnings are not set as unapproved!");    
    }

    [Then(@"the short course learner is not returned in the earnings response")]
    public void ThenTheShortCourseLearnerIsNotReturnedInTheEarningsResponse()
    {
        var testData = context.Get<TestData>();

        var learner = testData.ShortCourseEarningsResponse.Learners.SingleOrDefault(x => x.LearningKey == testData.ShortCourseLearnerKey.ToString());
        Assert.IsNull(learner, "Short course learner was unexpectedly found in the earnings response for this collection period.");
    }

    [Then(@"the funding line type for the short course is (.*)")]
    public void ThenTheFundingLineTypeForTheShortCourseIs(string expectedFundingLineType)
    {
        var testData = context.Get<TestData>();

        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[Constants.UkPrn];
        var courseCode = shortCourseRequest.Delivery.OnProgramme.Single().CourseCode;

        var shortCourseLearningKey = learningSqlClient.GetShortCourseLearning(testData.Uln).GetEpisode(Constants.UkPrn, courseCode).LearningKey;

        var learnerCount = testData.ShortCourseEarningsResponse?.Learners?.Count(x => x.LearningKey == shortCourseLearningKey.ToString()) ?? 0;
        Assert.AreEqual(1, learnerCount, "Short course learner was expected exactly once in the earnings response but found a different count.");

        var learner = testData.ShortCourseEarningsResponse.Learners.Single(x => x.LearningKey == shortCourseLearningKey.ToString());
        Assert.AreEqual(expectedFundingLineType, learner.Courses.First().FundingLineType, "Funding Line Type does not match expected value.");
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
        await ValidateTheShortCourseDataIsSentToApprovals();
    }

    [Then("notify approvals of learner for provider (.*)")]
    public async Task ThenNotifyApprovalsOfThisLearner(string provider)
    {
        long ukPrn = UkprnProvider.GetUkprnForProvider(provider);
        await ValidateTheShortCourseDataIsSentToApprovals(ukPrn);
    }

    public async Task ValidateTheShortCourseDataIsSentToApprovals(long ukprn = Constants.UkPrn)
    {
        var testData = context.Get<TestData>();
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];
        var shortCourseOnProgramme = shortCourseRequest.Delivery.OnProgramme.Single();

        List<ShortCourseEarningsModel>? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Count > 0;
        }, "Failed to find short course earnings entity.");

        await WaitHelper.WaitForIt(() =>
        {
            var publishedEvent = LearnerDataEventHandler.GetMessage(x => x.ULN == long.Parse(testData.Uln));
            if (publishedEvent != null)
            {
                Assert.AreEqual(ukprn, publishedEvent.UKPRN, "UKPRN does not match");
                Assert.AreEqual(LearnerData.Events.LearningType.ApprenticeshipUnit, publishedEvent.LearningType, "LearningType does not match");
                //Assert.AreEqual(shortCourseOnProgramme.CourseCode, publishedEvent.StandardCode.ToString(), "StandardCode does not match"); TODO assert this correctly when we build 1607, might be called LARSCode on the event
                Assert.AreEqual((int)earningsModel!.GetEpisode(ukprn, shortCourseOnProgramme.CourseCode).CoursePrice, publishedEvent.TrainingPrice, "TrainingPrice does not match CoursePrice");

                return true;
            }
            return false;
        }, "Failed to find published LearnerDataEvent.");
    }

    [Then("the learner ref is not stored and the short course data is not sent to approvals")]
    public async Task ThenTheLearnerRefIsNotStoredAndShortCourseDataIsNotSentToApprovals()
    {
        var testData = context.Get<TestData>();

        await Task.WhenAll(
            WaitHelper.WaitForUnexpected(() =>
                learningSqlClient.GetShortCourseLearning(testData.Uln.ToString()) != null,
                "Found unexpected learner data for learner ref"),
            WaitHelper.WaitForUnexpected(() =>
                LearnerDataEventHandler.GetMessage(x => x.ULN == long.Parse(testData.Uln)) != null,
                "Found unexpected LearnerDataEvent.")
        );
    }

    [Then(@"(.*) earnings profile history records are created for the short course")]
    public async Task ThenEarningsProfileHistoryRecordsAreCreatedForTheShortCourse(int expectedRecordCount)
    {
        var testData = context.Get<TestData>();
        List<ShortCourseEarningsModel>? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
                if (earningsModel == null) return false;

                var episodes = earningsModel;
                if (episodes == null || !episodes.Any()) return false;

                var latestEpisode = episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent);
                if (latestEpisode.EarningsProfile == null) return false;

                return latestEpisode.EarningsProfileHistory != null && latestEpisode.EarningsProfileHistory.Count == expectedRecordCount;
            }, $"Failed to find exactly {expectedRecordCount} history records.");

        var latestEpisode = earningsModel!.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent);
        var historyRecords = latestEpisode.EarningsProfileHistory;

        foreach (var history in historyRecords)
        {
            Assert.AreEqual(latestEpisode.EarningsProfile.EarningsProfileId, history.EarningsProfileId, "EarningsProfileId in history does not match current EarningsProfileId");
            Assert.AreNotEqual(Guid.Empty, history.Version, "Version is empty in history record");
        }
    }

    [Then(@"inform approvals that the learner has been withdrawn from the short course")]
    public async Task ThenInformApprovalsThatTheLearnerHasBeenWithdrawnFromTheShortCourse()
    {
        var testData = context.Get<TestData>();

        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[Constants.UkPrn];
        var courseCode = shortCourseRequest.Delivery.OnProgramme.Single().CourseCode;

        var shortCourseLearningKey = learningSqlClient.GetShortCourseLearning(testData.Uln).GetEpisode(Constants.UkPrn, courseCode).LearningKey;

        await context.ReceiveLearningWithdrawnEvent(shortCourseLearningKey);

        var expectedLastDayOfLearning = shortCourseRequest.Delivery.OnProgramme.Single().WithdrawalDate;
        var expectedWithdrawalReason = shortCourseRequest.Delivery.OnProgramme.Single().WithdrawalReasonCode;

        Assert.AreEqual(expectedLastDayOfLearning?.Date, testData.LearningWithdrawnEvent.WithdrawalDate.Date, "Unexpected last day of learning found in the event!");
        Assert.AreEqual(expectedWithdrawalReason, testData.LearningWithdrawnEvent.WithdrawalReasonCode, "Unexpected Withdrawal Reason Code found in the event!");
    }

    [Then(@"inform payments that the learner has been withdrawn from the short course")]
    public async Task ThenInformPaymentsThatTheLearnerHasBeenWithdrawnFromTheShortCourse()
    {
        var testData = context.Get<TestData>();
        
        await WaitHelper.WaitForIt(() =>
        {
            var course = learningSqlClient.GetShortCourseLearning(testData.Uln);
            var learnerKey = learningSqlClient.GetShortCourseLearning(testData.Uln)?.FirstOrDefault()?.Learner.Key;
            var command = GrowthAndSkillsPaymentsRecalculatedEventHandler
                .GetMessage(x => x.Command.Learner.LearnerKey == learnerKey)
                ?.Command;

            testData.CalculateGrowthAndSkillsPaymentsCommand = command ?? testData.CalculateGrowthAndSkillsPaymentsCommand;
            
            return testData.CalculateGrowthAndSkillsPaymentsCommand != null && 
                   testData.CalculateGrowthAndSkillsPaymentsCommand.Training.TrainingStatus.ToString() == "Withdrawn";
        }, "Failed to find the withdrawn training status in the growth and skills payments recalculated event command.");
    }

[Then(@"send empty earnings command to payments")]
public async Task ValidateEmptyEarningsCommandIsSentToPayments()
{
    var testData = context.Get<TestData>();

    await WaitHelper.WaitForIt(() =>
    {
        var course = learningSqlClient.GetShortCourseLearning(testData.Uln);
        var learnerKey = learningSqlClient.GetShortCourseLearning(testData.Uln)?.FirstOrDefault()?.Learner.Key;
        var command = GrowthAndSkillsPaymentsRecalculatedEventHandler
            .GetMessage(x => x.Command.Learner.LearnerKey == learnerKey)
            ?.Command;

        testData.CalculateGrowthAndSkillsPaymentsCommand = command ?? testData.CalculateGrowthAndSkillsPaymentsCommand;

        return testData.CalculateGrowthAndSkillsPaymentsCommand != null &&
               !testData.CalculateGrowthAndSkillsPaymentsCommand.Earnings.Any();
    }, "Failed to find the withdrawn training status in the growth and skills payments recalculated event command or Earnings are not empty!.");
}

 [Then("send the payable 30% milestone earnings to payments")]
 public async Task SendThePayableMilestoneEarningsToPayments()
 {
     var testData = context.Get<TestData>();

     await WaitHelper.WaitForIt(() =>
     {
         var course = learningSqlClient.GetShortCourseLearning(testData.Uln);
         var learnerKey = learningSqlClient.GetShortCourseLearning(testData.Uln)?.FirstOrDefault()?.LearnerKey;
         var command = GrowthAndSkillsPaymentsRecalculatedEventHandler
             .GetMessage(x => x.Command.Learner.LearnerKey == learnerKey)
             ?.Command;

         testData.CalculateGrowthAndSkillsPaymentsCommand = command ?? testData.CalculateGrowthAndSkillsPaymentsCommand;

         return testData.CalculateGrowthAndSkillsPaymentsCommand != null &&
                testData.CalculateGrowthAndSkillsPaymentsCommand.Earnings.Count() == 1 &&
                testData.CalculateGrowthAndSkillsPaymentsCommand.Earnings.First()
                .PricePeriods.First()
                .Periods.First()
                .EarningType == EarningType.Milestone1;
     }, "Failed to find the milestone1 earnings in the growth and skills payments event command.");
 }


    [Then("short course is marked as removed from learning and earning dbs")]
    public async Task ShortCourseIsMarkedAsRemovedFromLearningAndEarningDbs()
    {
        var testData = context.Get<TestData>();

        var shortCourseLearning = learningSqlClient.GetShortCourseLearning(testData.Uln);

        var shortCourseEarnings = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());

        Assert.IsTrue(shortCourseLearning.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)?.IsRemoved, "Short course learning episode NOT marked as removed.");
        Assert.IsTrue(shortCourseEarnings.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)?.IsRemoved, "Short course earnings episode NOT marked as removed.");
    }

    [Then("short course learning is reinstated in learning and earning dbs")]
    public void ShortCourseLearningIsReinstatedInLearningAndEarningDbs()
    {
        var testData = context.Get<TestData>();

        var shortCourseLearning = learningSqlClient.GetShortCourseLearning(testData.Uln);

        var shortCourseEarnings = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());

        Assert.IsFalse(shortCourseLearning.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)?.IsRemoved, "Short course learning episode NOT reinstated.");
        Assert.IsFalse(shortCourseEarnings.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)?.IsRemoved, "Short course earnings episode NOT reinstated.");
    }

    [Then("learning contains an epidose for Provider A and an episode for Provider B")]
    public void ThenLearningContainsAnEpidoseForProviderAAndAnEpisodeForProviderB()
    {
        var testData = context.Get<TestData>();
        var learningRecord = learningSqlClient.GetShortCourseLearning(testData.Uln);

        learningRecord.SelectMany(l => l.Episodes)
            .Should()
            .HaveCount(2, "There should be 2 episodes in the learning record, one for each provider.");

        learningRecord.Should().Contain(
            l => l.Episodes.Any(e => e.Ukprn == Constants.UkPrn), 
            "One episode should be for Provider A.");

        learningRecord.Should().Contain(
            l => l.Episodes.Any(e => e.Ukprn == Constants.AlternativeUkPrn),
            "One episode should be for Provider B.");
    }

    [Then("earnings contains an episode for Provider A and an episode for Provider B")]
    public void ThenEarningsContainsAnEpisodeForProviderAAndAnEpisodeForProviderB()
    {
        var testData = context.Get<TestData>();
        var earningRecord = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);

        earningRecord.SelectMany(l => l.Episodes)
            .Should()
            .HaveCount(2, "There should be 2 episodes in the learning record, one for each provider.");

        earningRecord.Should().Contain(
            e => e.Episodes.Any(e => e.Ukprn == Constants.UkPrn), 
            "One episode should be for Provider A.");

        earningRecord.Should().Contain(
            e => e.Episodes.Any(e => e.Ukprn == Constants.AlternativeUkPrn), 
            "One episode should be for Provider B.");
    }

    enum InstalmentState { DoesNotExist, ExistsButNotPayable, Payable };
    [Then("earnings instalments are calculated as follows")]
    public void ThenEarningsInstalmentsAreCalculatedAsFollows(Table table)
    {
        var testData = context.Get<TestData>();

        var earningRecord = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);

        foreach (var row in table.Rows)
        {
            var provider = row["Provider"];
            var ukprn = UkprnProvider.GetUkprnForProvider(provider);

            var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];
            var courseCode = shortCourseRequest.Delivery.OnProgramme.Single().CourseCode;

            var thirtyPercent = Enum.Parse<InstalmentState>(row["ThirtyPercent"]);
            var completion = Enum.Parse<InstalmentState>(row["Completion"]);

            var episode = earningRecord.GetEpisode(ukprn, courseCode);

            ValidateShortCourseInstalmentState(episode, "ThirtyPercentLearningComplete", thirtyPercent, provider);
            ValidateShortCourseInstalmentState(episode, "LearningComplete", completion, provider);
        }
    }

    private void ValidateShortCourseInstalmentState(ShortCourseEpisodeModel episode, string instalmentType, InstalmentState instalmentState, string provider)
    {
        if (instalmentState == InstalmentState.DoesNotExist)
            episode.EarningsProfile.Instalments.Should().NotContain(i => i.Type == instalmentType, $"{instalmentType} instalment should not exist for provider {provider}");
        else
        {
            var instalment = episode.EarningsProfile.Instalments.SingleOrDefault(i => i.Type == instalmentType);

            instalment.Should().NotBeNull($"{instalmentType} should exist {provider}");

            if (instalmentState == InstalmentState.Payable)
                instalment!.IsPayable.Should().BeTrue($"{instalmentType} should be payable {provider}");

            if (instalmentState == InstalmentState.ExistsButNotPayable)
                instalment!.IsPayable.Should().BeFalse($"{instalmentType} should not be payable {provider}");
        }
    }
}