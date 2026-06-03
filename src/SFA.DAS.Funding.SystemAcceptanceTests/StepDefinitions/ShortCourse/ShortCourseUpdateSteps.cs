using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.ShortCourse;

[Binding]
public class ShortCourseUpdateSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper)
{
    [Given(@"the training provider recorded that the 30% milestone has been reached")]
    [When(@"the training provider recorded that the 30% milestone has been reached")]
    public async Task GivenTheTrainingProviderRecordedThatThe30PercentMilestoneHasBeenReached()
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];

        var builder = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(shortCourseRequest.Delivery.OnProgramme.Single().StartDate)
            .WithEndDate(shortCourseRequest.Delivery.OnProgramme.Single().ExpectedEndDate)
            .WithMilestone(LearnerDataOuterApiClient.Milestone.ThirtyPercentLearningComplete);

        var updatedRequest = builder.Build();
        await learnerDataOuterApiHelper.UpdateShortCourseLearning(ukprn, testData.ShortCourseLearningKey, updatedRequest);
        testData.ShortCourseCreateUpdateRequests[ukprn] = updatedRequest;
        testData.ExpectGrowthAndSkillsPaymentsEvent = true;
    }

    [Given(@"the training provider also recorded that the learner completed")]
    [Given(@"the training provider recorded that the learner completed")]
    [When(@"the training provider recorded that the learner completed")]
    [When(@"the training provider also recorded that the learner completed")]
    public async Task GivenTheTrainingProviderAlsoRecordedThatTheLearnerCompleted()
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];

        var builder = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(shortCourseRequest.Delivery.OnProgramme.Single().StartDate)
            .WithEndDate(shortCourseRequest.Delivery.OnProgramme.Single().ExpectedEndDate)
            .WithMilestone(LearnerDataOuterApiClient.Milestone.ThirtyPercentLearningComplete)
            .WithCompletionDate(shortCourseRequest.Delivery.OnProgramme.Single().ExpectedEndDate);

        var updatedRequest = builder.Build();
        await learnerDataOuterApiHelper.UpdateShortCourseLearning(ukprn, testData.ShortCourseLearningKey, updatedRequest);
        testData.ShortCourseCreateUpdateRequests[ukprn] = updatedRequest;
        testData.ExpectGrowthAndSkillsPaymentsEvent = true;
    }

    [When(@"SLD inform us that the learner has withdrawn")]
    public async Task WhenSLDInformUsThatTheLearnerHasWithdrawn()
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];

        shortCourseRequest.Delivery.OnProgramme.Single().WithdrawalDate = DateTime.Now;
        shortCourseRequest.Delivery.OnProgramme.Single().CompletionDate = null; 
        shortCourseRequest.Delivery.OnProgramme.Single().Milestones.Remove(LearnerDataOuterApiClient.Milestone.LearningComplete);

        await learnerDataOuterApiHelper.UpdateShortCourseLearning(ukprn, testData.ShortCourseLearningKey, shortCourseRequest);
        testData.ExpectGrowthAndSkillsPaymentsEvent = true;
    }

    [When(@"SLD inform us that the learner has withdrawn with both milestones removed")]
    public async Task SLDInformUsThatTheLearnerHasWithdrawnWithBothMilestonesRemoved()
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];

        shortCourseRequest.Delivery.OnProgramme.Single().WithdrawalDate = DateTime.Now;
        shortCourseRequest.Delivery.OnProgramme.Single().CompletionDate = null;
        shortCourseRequest.Delivery.OnProgramme.Single().Milestones.Remove(LearnerDataOuterApiClient.Milestone.ThirtyPercentLearningComplete);

        await learnerDataOuterApiHelper.UpdateShortCourseLearning(ukprn, testData.ShortCourseLearningKey, shortCourseRequest);
        testData.ExpectGrowthAndSkillsPaymentsEvent = true;
    }

    [When(@"SLD also inform us that the 30% milestone was removed")]
    public async Task WhenSLDAlsoInformUsThatThe30PercentMilestoneWasRemoved()
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];

        shortCourseRequest.Delivery.OnProgramme.Single().Milestones = [];

        await learnerDataOuterApiHelper.UpdateShortCourseLearning(ukprn, testData.ShortCourseLearningKey, shortCourseRequest);
        testData.ExpectGrowthAndSkillsPaymentsEvent = true;
    }

    [When(@"SLD also inform us that the 30% milestone was not removed")]
    public void WhenSLDAlsoInformUsThatThe30PercentMilestoneWasNotRemoved()
    {
        // No action needed
    }

    [When(@"SLD inform us that the learner has been removed")]
    public async Task WhenSLDInformUsThatTheLearnerHasBeenRemoved()
    {
        var testData = context.Get<TestData>();
        await learnerDataOuterApiHelper.DeleteShortCourse(Constants.UkPrn, testData.ShortCourseLearningKey);
    }

    [Given(@"Provider (.*) has recorded 30% (.*) and completion (.*)")]
    [When(@"Provider (.*) has recorded 30% (.*) and completion (.*)")]
    public async Task UpdateMilestones(string provider, string thirtyPercent, string completion)
    {
        var testData = context.Get<TestData>();
        var ukprn = UkprnProvider.GetUkprnForProvider(provider);
        var shortCourseRequest = testData.ShortCourseCreateUpdateRequests[ukprn];

        var milestones = new List<LearnerDataOuterApiClient.Milestone>();

        if (thirtyPercent.Equals("isPayable", StringComparison.OrdinalIgnoreCase))
            milestones.Add(LearnerDataOuterApiClient.Milestone.ThirtyPercentLearningComplete);

        if (completion.Equals("isPayable", StringComparison.OrdinalIgnoreCase))
            milestones.Add(LearnerDataOuterApiClient.Milestone.ThirtyPercentLearningComplete);

        shortCourseRequest.Delivery.OnProgramme.Single().Milestones = milestones;

        await learnerDataOuterApiHelper.UpdateShortCourseLearning(ukprn, testData.ShortCourseLearningKey, shortCourseRequest);

    }
}
