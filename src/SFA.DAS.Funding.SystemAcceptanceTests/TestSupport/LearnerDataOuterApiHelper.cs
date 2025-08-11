using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    public class LearnerDataOuterApiHelper
    {
        private readonly LearnerDataOuterApiClient _apiClient = new();

        public async Task<LearnerDataRequest> AddLearnerData(string uln, long ukprn, int academicYear)
        {
            var fixture = new Fixture();
            var learnerData = new List<LearnerDataOuterApiClient.LearnerDataRequest>
            {
                fixture.Build<LearnerDataOuterApiClient.LearnerDataRequest>()
                    .With(x => x.ULN, Convert.ToInt64(uln))
                    .With(x => x.UKPRN, ukprn)
                    .With(x => x.LearnerEmail, $"{uln}@test.com")
                    .With(x => x.StartDate, DateTime.UtcNow)
                    .With(x => x.PlannedEndDate, DateTime.UtcNow.AddYears(1))
                    .With(x => x.AgreementId, "AG1")
                    .With(x => x.StandardCode, 57)
                    .Create()
            };

            await _apiClient.AddLearnerData(ukprn, academicYear, learnerData);

            return learnerData.First();
        }

        public async Task CompleteLearning(Guid learningKey, DateTime? completionDate)
        {
            var requestData = new UpdateLearnerRequest()
            {
                Delivery = new UpdateLearnerRequestDeliveryDetails()
                {
                    CompletionDate = completionDate,
                    MathsAndEnglishCourses = []
                }
            };

            await _apiClient.UpdateLearning(learningKey, requestData);
        }

        public async Task AddMathsAndEnglish(Guid learningKey, MathsAndEnglish mathsAndEnglish)
        {
            var requestData = new UpdateLearnerRequest()
            {
                Delivery = new UpdateLearnerRequestDeliveryDetails()
                {
                    MathsAndEnglishCourses = new List<MathsAndEnglish>
                    {
                        mathsAndEnglish
                    }
                }
            };

            await _apiClient.UpdateLearning(learningKey, requestData);
        }
    }
}
