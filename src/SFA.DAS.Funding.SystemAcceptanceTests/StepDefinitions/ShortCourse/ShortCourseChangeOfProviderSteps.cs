using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.ShortCourse;

[Binding]
public class ShortCourseChangeOfProviderSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper)
{
    [Given("that a “short course” learner has been created by Provider (.*)")]
    [When("that a “short course” learner has been created by Provider (.*)")]
    public async Task GivenThatAShortCourseLearnerHasBeenCreatedByProvider(string trainingProvider)
    {
        var testData = context.Get<TestData>();
        var createShortCourseRequest = testData.ShortCourseLearnerData;

        if (createShortCourseRequest == null)
        {
            createShortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
                .WithStartDate(DateTime.Now.AddMonths(-2))
                .WithEndDate(DateTime.Now.AddMonths(2))
                .Build();
        }
        else
        {
            createShortCourseRequest.Delivery.OnProgramme.Single().StartDate = DateTime.Now;
        }

        long ukPrn = trainingProvider switch
        {
            "A" => Constants.UkPrn,
            "B" => Constants.AlternativeUkPrn,
            _ => throw new ArgumentException($"Invalid training provider - {trainingProvider}", nameof(trainingProvider))
        };

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(ukPrn, createShortCourseRequest);

        testData.ShortCourseLearnerData = createShortCourseRequest;
    }

    [Given("the learner has not completed the course with Provider (.*)")]
    public void GivenTheLearnerHasNotCompletedTheCourseWithProvider(string trainingProvider)
    {
        // Do nothing
    }

}