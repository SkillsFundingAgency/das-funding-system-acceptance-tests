using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.ShortCourse;

[Binding]
public class ShortCourseProgressionSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper, ShortCourseEarningsAndPaymentsAssertionHelper assertionHelper)
{
    [When(@"SLD submits a progression PUT for a new course alongside the existing course")]
    public async Task WhenSLDSubmitsAProgressionPUTForANewCourseAlongsideTheExistingCourse()
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var existingRequest = testData.ShortCourseCreateUpdateRequests[ukprn];
        var existingOnProgramme = existingRequest.Delivery.OnProgramme.Single();

        var newCourseOnProgramme = ShortCourseLearnerDataBuilder.CreateNew(testData)
            .WithCourseCode("ZSC00005")
            .WithStartDate(existingOnProgramme.ExpectedEndDate.AddDays(1))
            .WithEndDate(existingOnProgramme.ExpectedEndDate.AddMonths(3))
            .Build()
            .Delivery.OnProgramme.Single();

        testData.ProgressionCourseCode = newCourseOnProgramme.CourseCode;

        existingRequest.Delivery.OnProgramme = [existingOnProgramme, newCourseOnProgramme];

        await learnerDataOuterApiHelper.UpdateShortCourseLearning(ukprn, testData.ShortCourseLearnerKey, existingRequest);

        testData.ShortCourseCreateUpdateRequests[ukprn] = existingRequest;
    }

    [Then(@"unapproved earnings are generated for the new course")]
    public Task ThenUnapprovedEarningsAreGeneratedForTheNewCourse()
    {
        return assertionHelper.AssertUnapprovedEarningsGeneratedForNewCourse();
    }

    [Then(@"the original course earnings are unaffected")]
    public void ThenTheOriginalCourseEarningsAreUnaffected()
    {
        assertionHelper.AssertOriginalCourseEarningsUnaffected();
    }
}
