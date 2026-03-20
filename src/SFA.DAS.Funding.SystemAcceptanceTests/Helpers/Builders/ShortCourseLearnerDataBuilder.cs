using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;

public class ShortCourseLearnerDataBuilder(TestData testData)
{
    private readonly LearnerDataOuterApiClient.ShortCourseRequest _request = testData.ShortCourseLearnerData ?? new LearnerDataOuterApiClient.ShortCourseRequest
    {
        Learner = new LearnerDataOuterApiClient.ShortCourseLearnerRequestDetails
        {
            Uln = long.Parse(testData.Uln),
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
                    CourseCode = "109",
                    AgreementId = "SCAgreement1",
                    StartDate = new DateTime(2024, 08, 01),
                    ExpectedEndDate = new DateTime(2024, 11, 01),
                    LearningSupport = [],
                    Milestones = []
                }
            ]
        }
    };

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

    public LearnerDataOuterApiClient.ShortCourseRequest Build()
    {
        return _request;
    }
}