using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.ShortCourse;

[Binding]
public class ShortCourseProgressionSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper, ShortCourseEarningsAndPaymentsAssertionHelper assertionHelper)
{
    [When("SLD submits a progression PUT for a new course with start date (.*) alongside the existing course")]
    public async Task SLDSubmitsAProgressionPUTForANewCourseWithStartDateAlongsideTheExistingCourse(TokenisableDateTime startDate)
    {
        var testData = context.Get<TestData>();
        var ukprn = Constants.UkPrn;
        var existingRequest = testData.ShortCourseCreateUpdateRequests[ukprn];
        var existingOnProgramme = existingRequest.Delivery.OnProgramme.Single();

        var newCourseOnProgramme = ShortCourseLearnerDataBuilder.CreateNew(testData)
            .WithCourseCode("ZSC00005")
            .WithStartDate(startDate.Value)
            .WithEndDate(startDate.Value.AddMonths(3))
            .WithMilestone(LearnerDataOuterApiClient.Milestone.ThirtyPercentLearningComplete)
            .Build()
            .Delivery.OnProgramme.Single();

        testData.ProgressionCourseCode = newCourseOnProgramme.CourseCode;

        existingRequest.Delivery.OnProgramme = [existingOnProgramme, newCourseOnProgramme];

        await learnerDataOuterApiHelper.UpdateShortCourseLearning(ukprn, testData.ShortCourseLearnerKey, existingRequest);

        testData.ShortCourseCreateUpdateRequests[ukprn] = existingRequest;
    }

    [When("SLD submits a progression POST with the (same|different) provider for a new course in academic year (.*) with start date (.*)")]
    public async Task SLDSubmitsAProgressionPOSTForANewCourseInAcademicYearWithStartDate(string provider, TokenisableAcademicYear academicYear, TokenisableDateTime startDate)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("UKPRN cannot be null or empty.", nameof(provider));
        }

        var normalisedProvider = provider.Trim().ToLowerInvariant();

        if (normalisedProvider != "same" && normalisedProvider != "different")
        {
            throw new ArgumentException(
                $"Invalid periodNumber '{provider}'. Expected 'same' or 'different' (case-insensitive).",
                nameof(provider));
        }

        var testData = context.Get<TestData>();

        var ukprn = normalisedProvider == "same" ? Constants.UkPrn : Constants.AlternativeUkPrn;

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(startDate.Value)
            .WithEndDate(startDate.Value.AddMonths(3))
            .WithCourseCode("ZSC00009")
            .Build();

        shortCourseRequest.Delivery.OnProgramme.Single().Milestones = [];
        shortCourseRequest.Delivery.OnProgramme.Single().CompletionDate = null;
        shortCourseRequest.Delivery.OnProgramme.Single().WithdrawalDate = null;

        testData.ShortCourseCreateUpdateRequests[ukprn] = shortCourseRequest;
        testData.ProgressionCourseCode = shortCourseRequest.Delivery.OnProgramme.First().CourseCode;

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(ukprn, shortCourseRequest, academicYear.Value);
    }

    [Then(@"unapproved earnings with (.*) provider are generated for the new course")]
    public Task ThenUnapprovedEarningsAreGeneratedForTheNewCourse(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new ArgumentException("UKPRN cannot be null or empty.", nameof(provider));
        }

        var normalisedProvider = provider.Trim().ToLowerInvariant();

        if (normalisedProvider != "same" && normalisedProvider != "different")
        {
            throw new ArgumentException(
                $"Invalid periodNumber '{provider}'. Expected 'same' or 'different' (case-insensitive).",
                nameof(provider));
        }

        var ukprn = normalisedProvider == "same" ? Constants.UkPrn : Constants.AlternativeUkPrn;

        return assertionHelper.AssertUnapprovedEarningsGeneratedForNewCourse(ukprn);
    }

    [Then(@"approved earnings are generated for the new course")]
    public Task ThenApprovedEarningsAreGeneratedForTheNewCourse()
    {
        return assertionHelper.AssertApprovedEarningsGeneratedForNewCourse();
    }

    [Then(@"both original course earnings are unaffected")]
    public void BothOriginalCourseEarningsAreUnaffected()
    {
        assertionHelper.AssertOriginalCourseEarningsUnaffected(true);
    }

    [Then(@"30% original milestone earning is unaffected")]
    public void ThirtyPercentOriginalMilestoneEarningIsUnaffected()
    {
        assertionHelper.AssertOriginalCourseEarningsUnaffected(false);
    }
}
