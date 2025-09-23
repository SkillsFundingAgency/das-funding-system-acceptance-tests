using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    public class LearnerDataOuterApiHelper ()
    {
        private readonly LearnerDataOuterApiClient _apiClient = new();

        public async Task<LearnerDataRequest> AddLearnerData(string uln, long ukprn)
        {
            var fixture = new Fixture();
            var learnerData = new List<LearnerDataOuterApiClient.LearnerDataRequest>
            {
                fixture.Build<LearnerDataOuterApiClient.LearnerDataRequest>()
                    .With(x => x.Learner.Uln, uln)
                    .With(x => x.Learner.Email, $"{uln}@test.com")
                    .With(x => x.Delivery.OnProgramme.StartDate, DateTime.UtcNow)
                    .With(x => x.Delivery.OnProgramme.ExpectedEndDate, DateTime.UtcNow.AddYears(1))
                    .With(x => x.Delivery.OnProgramme.AgreementId, "AG1")
                    .With(x => x.Delivery.OnProgramme.StandardCode, 57)
                    .Create()
            };

            await _apiClient.AddLearnerData(ukprn, learnerData);

            return learnerData.First();
        }

        public async Task<GetLearnerResponse> GetLearnersForProvider (long ukprn, int academicYear)
        {
            return await _apiClient.GetLearners(ukprn, academicYear);
        }

        public async Task UpdateLearning(Guid learningKey, Action<LearnerDataBuilder> configure)
        {
            var builder = new LearnerDataBuilder();
            configure(builder);
            var request = builder.Build();

            await _apiClient.UpdateLearning(Constants.UkPrn,learningKey, request);
        }


        public async Task UpdateLearning(Guid learningKey, UpdateLearnerRequest request)
        {
            await _apiClient.UpdateLearning(Constants.UkPrn, learningKey, request);
        }
    }
}
