using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;

public class ShortCourseLearnerDataBuilder
{
    private readonly LearnerDataOuterApiClient.ShortCourseRequest _request;

    public ShortCourseLearnerDataBuilder(TestData testData)
    {
        if (testData.ShortCourseCreateUpdateRequests.Count > 1)
            throw new Exception("If multiple ShortCourseCreateUpdateRequests exist in TestData, the builder cannot determine which one to use. Use constructor (TestData testData, long ukprn) instead");

        if (testData.ShortCourseCreateUpdateRequests.Count == 1)
        {
            _request = testData.ShortCourseCreateUpdateRequests.Single().Value;
            return;
        }

        _request = BuildDefaultRequest(testData);
    }

    public ShortCourseLearnerDataBuilder(TestData testData, long ukprn)
    {
        _request = testData.ShortCourseCreateUpdateRequests[ukprn];
    }

    private ShortCourseLearnerDataBuilder(TestData testData, bool alwaysBuildFresh)
    {
        _request = BuildDefaultRequest(testData);
    }

    /// <summary>
    /// Always builds a brand new default request, regardless of what is already stored in TestData.
    /// Use this when representing a genuinely distinct course/provider request rather than an update
    /// to an existing one (e.g. a second course for the same learner, or a different provider's request).
    /// </summary>
    public static ShortCourseLearnerDataBuilder CreateNew(TestData testData) => new(testData, true);

    private static LearnerDataOuterApiClient.ShortCourseRequest BuildDefaultRequest(TestData testData)
    {
        return new LearnerDataOuterApiClient.ShortCourseRequest
        {
            Learner = new LearnerDataOuterApiClient.ShortCourseLearnerRequestDetails
            {
                Uln = long.Parse(testData.Uln),
                LearnerRef = "test",
                FirstName = "Short",
                LastName = "CourseLearner",
                Dob = new DateTime(2000, 1, 1),
                Email = "learner@test.com",
                HasEhcp = false
            },
            Delivery = new LearnerDataOuterApiClient.ShortCourseDelivery
            {
                OnProgramme =
            [
                new LearnerDataOuterApiClient.ShortCourseOnProgramme
                {
                    CourseCode = "ZSC00004",
                    AgreementId = "SCAgreement1",
                    StartDate = new DateTime(2026, 08, 01),
                    ExpectedEndDate = new DateTime(2026, 11, 01),
                    LearningSupport = [],
                    Milestones = []
                }
            ]
            }
        };
    }

    public ShortCourseLearnerDataBuilder WithStartDate(DateTime startDate)
    {
        _request.Delivery.OnProgramme.Single().StartDate = startDate;
        return this;
    }

    public ShortCourseLearnerDataBuilder WithEndDate(DateTime endDate)
    {
        _request.Delivery.OnProgramme.Single().ExpectedEndDate = endDate;
        return this;
    }

    public ShortCourseLearnerDataBuilder WithCompletionDate(DateTime completionDate)
    {
        _request.Delivery.OnProgramme.Single().CompletionDate = completionDate;
        return this;
    }

    public ShortCourseLearnerDataBuilder WithLearnerDetails(string firstName, string lastName, string email)
    {
        _request.Learner.FirstName = firstName;
        _request.Learner.LastName = lastName;
        _request.Learner.Email = email;
        return this;
    }

    public ShortCourseLearnerDataBuilder WithDateOfBirth(DateTime dob)
    {
        _request.Learner.Dob = dob;
        return this;
    }

    public ShortCourseLearnerDataBuilder WithMilestone(LearnerDataOuterApiClient.Milestone milestone)
    {
        _request.Delivery.OnProgramme.Single().Milestones.Add(milestone);

        return this;
    }

    public ShortCourseLearnerDataBuilder WithCourseCode(string courseCode)
    {
        _request.Delivery.OnProgramme.Single().CourseCode = courseCode;

        return this;
    }

    public LearnerDataOuterApiClient.ShortCourseRequest Build()
    {
        return _request;
    }
}