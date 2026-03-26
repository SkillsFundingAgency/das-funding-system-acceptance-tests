using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ShortCourseAddSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper)
{
    [Given(@"SLD informs us of a new learner with a short course starting on (.*)")]
    public async Task GivenANewLearnerWithAShortCourse(TokenisableDateTime startDate)
    {
        context.Set(new TestData(TestIdentifierProvider.GetNextUln()));
        await AddShortCourse(startDate.Value);
    }

    [Given(@"SLD informs us of a new learner with a short course starting on (.*) and ending on (.*)")]
    public async Task GivenANewLearnerWithAShortCourseExplicitEndDate(TokenisableDateTime startDate, TokenisableDateTime endDate)
    {
        context.Set(new TestData(TestIdentifierProvider.GetNextUln()));
        await AddShortCourse(startDate.Value, endDate.Value);
    }

    [When(@"SLD informs us of a short course for the learner starting on (.*)")]
    public async Task WhenTheProviderAddsAShortCourseForTheLearnerStartingOn(TokenisableDateTime startDate)
    {
        await AddShortCourse(startDate.Value);
    }

    private async Task AddShortCourse(DateTime startDate, DateTime? endDate = null)
    {
        endDate ??= startDate.AddMonths(3);

        var testData = context.Get<TestData>();

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(startDate)
            .WithEndDate(endDate.Value)
            .Build();

        context.Set(shortCourseRequest);

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);

        testData.ShortCourseLearnerData = shortCourseRequest;
    }

    [Given(@"SLD informs us of a the same new short course learner again")]
    public async Task GivenTheSameNewShortCourseLearner()
    {
        var shortCourseRequest = context.Get<LearnerDataOuterApiClient.ShortCourseRequest>();
        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);
    }

    [When(@"SLD informs us of a change to the short course dates pre approval")]
    public async Task WhenTheProviderChangesShortCourseDetailsPreApproval()
    {
        var testData = context.Get<TestData>();

        var newStartDate = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single().StartDate.AddMonths(2);
        var newEndDate = newStartDate.AddMonths(4);

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(newStartDate)
            .WithEndDate(newEndDate)
            .Build();

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);

        testData.ShortCourseLearnerData = shortCourseRequest;
    }

    [When(@"SLD informs us of a short course for the learner starting on (.*) with updated learner details")]
    public async Task WhenTheProviderAddsAShortCourseForTheLearnerInTheCurrentAcademicYearWithUpdatedLearnerDetails(TokenisableDateTime startDate, Table table)
    {
        var testData = context.Get<TestData>();
        
        var row = table.Rows[0];
        var firstName = row["FirstName"];
        var lastName = row["LastName"];
        var emailAddress = row["EmailAddress"];
        var dateOfBirth = DateTime.Parse(row["DateOfBirth"]);

        var endDate = startDate.Value.AddMonths(3);

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(startDate.Value)
            .WithEndDate(endDate)
            .WithLearnerDetails(firstName, lastName, emailAddress)
            .WithDateOfBirth(dateOfBirth)
            .Build();

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);

        testData.ShortCourseLearnerData = shortCourseRequest;
    }

    [When(@"SLD informs us the short course learning has completed on (.*)")]
    public async Task WhenSLDInformsUsTheShortCourseLearningHasCompletedOn(TokenisableDateTime completionDate)
    {
        var testData = context.Get<TestData>();

        var shortCourseRequest = testData.ShortCourseLearnerData;
        shortCourseRequest.Delivery.OnProgramme.Single().CompletionDate = completionDate.Value;

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);
    }
}
