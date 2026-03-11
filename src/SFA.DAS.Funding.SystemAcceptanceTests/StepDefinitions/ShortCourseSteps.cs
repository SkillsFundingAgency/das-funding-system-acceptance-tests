using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ShortCourseSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper)
{
    [When(@"SLD informs us of a short course for the learner starting in (.*)")]
    public async Task WhenTheProviderAddsAShortCourseForTheLearnerInTheCurrentAcademicYear(TokenisableDateTime startDate)
    {
        var testData = context.Get<TestData>();
        
        var endDate = startDate.AddMonths(3);

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(startDate)
            .WithEndDate(endDate)
            .Build();

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);

        testData.ShortCourseLearnerData = shortCourseRequest;
    }

    [Then(@"the basic short course earnings are generated")]
    public void ThenTheShortCourseIsSuccessfullyProcessed()
    {
        var testData = context.Get<TestData>();
        Assert.IsNotNull(testData.ShortCourseLearnerData);
        //todo
    }
}
