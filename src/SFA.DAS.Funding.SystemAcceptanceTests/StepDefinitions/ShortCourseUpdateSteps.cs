using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ShortCourseUpdateSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper)
{
    [Given(@"the training provider recorded that the 30% milestone has been reached")]
    public async Task GivenTheTrainingProviderRecordedThatThe30PercentMilestoneHasBeenReached()
    {
        var testData = context.Get<TestData>();
        var shortCourseRequest = testData.ShortCourseLearnerData;
        
        var builder = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(shortCourseRequest.Delivery.OnProgramme.Single().StartDate)
            .WithEndDate(shortCourseRequest.Delivery.OnProgramme.Single().ExpectedEndDate)
            .WithMilestone(LearnerDataOuterApiClient.Milestone.ThirtyPercentLearningComplete);

        var updatedRequest = builder.Build();
        await learnerDataOuterApiHelper.UpdateShortCourseLearning(Constants.UkPrn, testData.ShortCourseLearningKey, updatedRequest);
        testData.ShortCourseLearnerData = updatedRequest;
    }

    [Given(@"the training provider also recorded that the learner completed")]
    [Given(@"the training provider recorded that the learner completed")]
    public async Task GivenTheTrainingProviderAlsoRecordedThatTheLearnerCompleted()
    {
        var testData = context.Get<TestData>();
        var shortCourseRequest = testData.ShortCourseLearnerData;

        var builder = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(shortCourseRequest.Delivery.OnProgramme.Single().StartDate)
            .WithEndDate(shortCourseRequest.Delivery.OnProgramme.Single().ExpectedEndDate)
            .WithCompletionDate(shortCourseRequest.Delivery.OnProgramme.Single().ExpectedEndDate)
            .WithMilestone(LearnerDataOuterApiClient.Milestone.LearningComplete);

        var updatedRequest = builder.Build();
        await learnerDataOuterApiHelper.UpdateShortCourseLearning(Constants.UkPrn, testData.ShortCourseLearningKey, updatedRequest);
        testData.ShortCourseLearnerData = updatedRequest;
    }


    [When(@"SLD inform us that the learner has withdrawn")]
    public async Task WhenSLDInformUsThatTheLearnerHasWithdrawn()
    {
        var testData = context.Get<TestData>();
        var shortCourseRequest = testData.ShortCourseLearnerData;
        
        shortCourseRequest.Delivery.OnProgramme.Single().WithdrawalDate = DateTime.Now;
        shortCourseRequest.Delivery.OnProgramme.Single().CompletionDate = null; 
        shortCourseRequest.Delivery.OnProgramme.Single().Milestones.Remove(LearnerDataOuterApiClient.Milestone.LearningComplete);

        await learnerDataOuterApiHelper.UpdateShortCourseLearning(Constants.UkPrn, testData.ShortCourseLearningKey, shortCourseRequest);
    }

    [When(@"SLD also inform us that the 30% milestone was removed")]
    public async Task WhenSLDAlsoInformUsThatThe30PercentMilestoneWasRemoved()
    {
        var testData = context.Get<TestData>();
        var shortCourseRequest = testData.ShortCourseLearnerData;
        
        shortCourseRequest.Delivery.OnProgramme.Single().Milestones = [];

        await learnerDataOuterApiHelper.UpdateShortCourseLearning(Constants.UkPrn, testData.ShortCourseLearningKey, shortCourseRequest);
    }

    [When(@"SLD also inform us that the 30% milestone was not removed")]
    public void WhenSLDAlsoInformUsThatThe30PercentMilestoneWasNotRemoved()
    {
        // No action needed
    }
}
