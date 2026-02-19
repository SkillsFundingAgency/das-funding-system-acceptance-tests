using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.Requests;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders
{
    public class ShortCourseRequestBuilder(TestData testData)
    {
        private readonly ShortCourseRequest _request = new()
        {
            Learner = new ShortCourseLearnerRequestDetails
            {
                Uln = long.Parse(testData.Uln),
                FirstName = "Sys acceptance",
                LastName = "Test",
                Dob = DateTime.UtcNow.AddYears(-25),
                Email = "test@test.com",
                HasEhcp = false
            },
            Delivery = new ShortCourseDelivery
            {
                
                OnProgramme =
                [
                    new ShortCourseOnProgramme
                    {
                        AgreementId = "1",
                        AimSequenceNumber = 1,
                        StartDate = DateTime.UtcNow.AddYears(-1),
                        ExpectedEndDate = DateTime.UtcNow.AddYears(1),
                        CourseCode = "91"
                    }
                ]
            }
        };

        public ShortCourseRequestBuilder WithStartDate(DateTime startDate)
        {
            _request.Delivery.OnProgramme.Latest().StartDate = startDate;
            return this;
        }

        public ShortCourseRequestBuilder WithExpectedEndDate(DateTime endDate)
        {
            _request.Delivery.OnProgramme.Latest().ExpectedEndDate = endDate;
            return this;
        }

        public ShortCourseRequestBuilder WithCourseCode(string courseCode)
        {
            _request.Delivery.OnProgramme.Latest().CourseCode = courseCode;
            return this;
        }

        public ShortCourseRequest Build()
        {
            return _request;
        }

    }
}
