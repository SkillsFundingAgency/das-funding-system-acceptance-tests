using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    public class LearnerDataOuterApiHelper
    {
        private readonly LearnerDataOuterApiClient _apiClient = new();
        private ScenarioContext _context;

        public void SetContext(ScenarioContext context)
        {
            _context = context;
        }


        public async Task<LearnerDataRequest> AddLearnerData(string uln, long ukprn)
        {
            var fixture = new Fixture();
            var learnerData =
                new LearnerDataRequest
                {
                    ConsumerReference = fixture.Create<string>(),
                    Learner = fixture.Build<StubLearner>()
                    .With(x => x.Uln, uln)
                    .With(x => x.Email, $"{uln}@test.com")
                    .Create(),
                    Delivery = new StubDelivery
                    {
                        EnglishAndMaths = fixture.Create<List<StubEnglishAndMaths>>(),
                        OnProgramme = fixture.Build<StubOnProgramme>()
                        .With(x => x.StartDate, DateTime.UtcNow)
                        .With(x => x.ExpectedEndDate, DateTime.UtcNow.AddYears(1))
                        .With(x => x.AgreementId, "AG1")
                        .With(x => x.StandardCode, 57)
                        .With(x => x.Costs, new List<CostDetails> { fixture.Create<CostDetails>() })
                        .With(x => x.LearningSupport, fixture.Create<List<LearningSupport>>())
                        .Create()
                    }
                };

            await _apiClient.AddLearnerData(ukprn, learnerData);

            return learnerData;
        }

        public async Task<GetLearnerResponse> GetLearnersForProvider(long ukprn, int academicYear)
        {
            return await _apiClient.GetLearners(ukprn, academicYear);
        }

        public async Task UpdateLearning(Guid learningKey, Action<LearnerDataBuilder> configure)
        {
            var builder = new LearnerDataBuilder(_context.Get<TestData>());
            configure(builder);
            var request = builder.Build();

            await _apiClient.UpdateLearning(Constants.UkPrn, learningKey, request);
        }


        public async Task UpdateLearning(Guid learningKey, UpdateLearnerRequest request)
        {
            await _apiClient.UpdateLearning(Constants.UkPrn, learningKey, request);
        }

        public async Task RemoveLearner(Guid learningKey)
        {
            await _apiClient.DeleteLearner(Constants.UkPrn, learningKey);
        }
    }
}
