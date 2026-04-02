using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ShortCourseAssertionSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient, LearningSqlClient learningSqlClient)
{
    [Then(@"remove all earnings for that ""short course""")]
    public async Task ThenRemoveAllEarningsForThatShortCourse()
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.All(x => !x.IsPayable) ?? true;
        }, "Failed to verify that all earnings were removed.");
    }

    [Then(@"remove the remaining completion earning")]
    [Then(@"remove the completion earning")]
    [Then(@"a completion earning is not generated")]
    public async Task ThenRemoveTheRemainingCompletionEarning()
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
            return instalments != null && instalments.All(x => x.Type != "LearningComplete" || !x.IsPayable);
        }, "Failed to verify that the completion earning was removed.");
    }

    [Then(@"remove the 30% milestone earning")]
    [Then(@"a 30% milestone earning is not generated")]
    public async Task ThenRemoveThe30PercentMilestoneEarning()
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
            return instalments != null && instalments.All(x => x.Type != "ThirtyPercentLearningComplete" || !x.IsPayable);
        }, "Failed to verify that the 30% milestone earning was removed.");
    }

    [Then(@"retain the 30% milestone earning")]
    [Then(@"a 30% milestone earning is generated")]
    public async Task ThenRetainThe30PercentMilestoneEarning()
    {
        var testData = context.Get<TestData>();

        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
            return instalments != null && instalments.Any(x => x.Type == "ThirtyPercentLearningComplete" && x.IsPayable);
        }, "Failed to find short course earnings with 30% milestone earning.");

        var instalment = earningsModel!.Episodes.Single().EarningsProfile.Instalments.Single(x => x.Type == "ThirtyPercentLearningComplete" && x.IsPayable);
        var totalPrice = earningsModel.Episodes.Single().CoursePrice;
        var expectedAmount = Math.Round(totalPrice * 0.3m, 5);

        Assert.AreEqual((double)expectedAmount, (double)instalment.Amount, 0.01, "The retained instalment is not the 30% milestone earning.");
    }

    [Then(@"a completion earning is generated")]
    public async Task ThenACompletionEarningIsGenerated()
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
            return instalments != null && instalments.Any(x => x.Type == "LearningComplete" && x.IsPayable);
        }, "Failed to verify that the completion earning was generated.");
    }

    [Given(@"the basic short course earnings are generated")]
    [Then(@"the basic short course earnings are generated")]
    public async Task ThenTheShortCourseIsSuccessfullyProcessed()
    {
        var testData = context.Get<TestData>();

        var expectedCourse = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();
        var expectedStartDate = expectedCourse.StartDate;
        var expectedEndDate = expectedCourse.ExpectedEndDate;

        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;
        }, "Failed to find short course earnings entity.");

        var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;

        var duration = (expectedEndDate - expectedStartDate).Days + 1;
        var daysToFirstPayment = (int)Math.Floor(duration * 0.3);
        var firstPaymentDate = expectedStartDate.AddDays(daysToFirstPayment);
        var secondPaymentDate = expectedEndDate;

        var expectedFirstPeriod = TableExtensions.Period[firstPaymentDate.ToString("MMMM")];
        var expectedFirstAcademicYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", firstPaymentDate));

        var expectedSecondPeriod = TableExtensions.Period[secondPaymentDate.ToString("MMMM")];
        var expectedSecondAcademicYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", secondPaymentDate));

        var firstInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedFirstPeriod && x.AcademicYear == expectedFirstAcademicYear);
        Assert.IsNotNull(firstInstalment, $"Could not find first instalment in period {expectedFirstPeriod} of AY {expectedFirstAcademicYear}");

        var secondInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedSecondPeriod && x.AcademicYear == expectedSecondAcademicYear);
        Assert.IsNotNull(secondInstalment, $"Could not find second instalment in period {expectedSecondPeriod} of AY {expectedSecondAcademicYear}");

        var totalPrice = earningsModel.Episodes.Single().CoursePrice;

        var expectedFirstAmount = Math.Round(totalPrice * 0.3m, 5);
        var expectedSecondAmount = Math.Round(totalPrice * 0.7m, 5);

        Assert.AreEqual((double)expectedFirstAmount, (double)firstInstalment.Amount, 0.01, "First instalment amount does not match exactly 30% of total price.");
        Assert.AreEqual((double)expectedSecondAmount, (double)secondInstalment.Amount, 0.01, "Second instalment amount does not match exactly 70% of total price.");
    }

    [Then(@"the short course earnings are set to approved")]
    public async Task ThenTheShortCourseEarningsAreSetToApproved()
    {
        var testData = context.Get<TestData>();

        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile != null;
        }, "Failed to find short course earnings entity.");

        Assert.IsTrue(earningsModel!.Episodes.Single().EarningsProfile.IsApproved, "Short course earnings should be approved.");
    }

    [Then("the short course earnings do not contain duplicates")]
    public async Task ThenTheShortCourseEarningsAreGeneratedWithoutDuplication()
    {
        var testData = context.Get<TestData>();
        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;
        }, "Failed to find short course earnings entity.");
        var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;
        Assert.AreEqual(2, instalments.Count, "Expected exactly 2 instalments for the short course, but found a different count.");
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
        var testData = context.Get<TestData>();

        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;
        }, "Failed to find short course earnings entity.");

        var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;

        var expectedSecondPeriod = period.Value.PeriodValue;
        var expectedSecondAcademicYear = period.Value.AcademicYear;

        var secondInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedSecondPeriod && x.AcademicYear == expectedSecondAcademicYear);
        Assert.IsNotNull(secondInstalment, $"Could not find second instalment in period {expectedSecondPeriod} of AY {expectedSecondAcademicYear}");

        var totalPrice = earningsModel.Episodes.Single().CoursePrice;
        var expectedSecondAmount = Math.Round(totalPrice * 0.7m, 5);

        Assert.AreEqual((double)expectedSecondAmount, (double)secondInstalment.Amount, 0.01, "Second instalment amount (70% of total price) not found in expected delivery period.");
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
        testData.ShortCourseEarningsResponse = await learnerDataOuterApiHelper.GetShortCourseEarningsData(Constants.UkPrn, period.Value.AcademicYear, (byte)period.Value.PeriodValue);
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
        var testData = context.Get<TestData>();
        
        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.Count > 0;
        }, "Failed to find short course earnings entity.");

        Assert.AreEqual(1, earningsModel!.Episodes.Count, "Expected exactly 1 episode for the earliest short course earnings, but found a different count.");
    }

    [Then(@"the earnings are still recorded against the first provider")]
    public async Task ThenTheEarningsAreStillRecordedAgainstTheFirstProvider()
    {
        var testData = context.Get<TestData>();
        
        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.Count > 0;
        }, "Failed to find short course earnings entity.");

        Assert.AreEqual(Constants.UkPrn, earningsModel!.Episodes.Single().Ukprn, "The earnings were not recorded against the first provider's UKPRN.");
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
            var publishedEvent = Infrastructure.Events.LearnerDataEventHandler.GetMessage(x => x.ULN == long.Parse(testData.Uln));
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
}
