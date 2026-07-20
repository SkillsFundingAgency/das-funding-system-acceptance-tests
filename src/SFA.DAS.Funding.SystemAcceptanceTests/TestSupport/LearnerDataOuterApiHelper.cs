using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    public class LearnerDataOuterApiHelper
    {
        private readonly LearnerDataOuterApiClient _apiClient = new();
        private readonly LearningSqlClient _learningSqlClient = new();
        private ScenarioContext _context;

        public void SetContext(ScenarioContext context)
        {
            _context = context;
        }


        public async Task<LearnerDataRequest> AddLearnerData(string uln, long ukprn)
        {
            var fixture = new Fixture();

            var onProgramme = fixture.Build<StubOnProgramme>()
                .With(x => x.StartDate, DateTime.UtcNow)
                        .With(x => x.ExpectedEndDate, DateTime.UtcNow.AddYears(1))
                        .With(x => x.AgreementId, "AG1")
                        .With(x => x.StandardCode, 57)
                        .With(x => x.Costs, new List<CostDetails> { fixture.Create<CostDetails>() })
                        .With(x => x.LearningSupport, fixture.Create<List<LearningSupport>>())
                        .Create();

            var learnerData = new LearnerDataRequest
                {
                    ConsumerReference = fixture.Create<string>(),
                    Learner = fixture.Build<StubLearner>()
                    .With(x => x.Uln, uln)
                    .With(x => x.Email, $"{uln}@test.com")
                    .Create(),
                    Delivery = new StubDelivery
                    {
                        EnglishAndMaths = fixture.Create<List<StubEnglishAndMaths>>(),
                        OnProgramme = new[] { onProgramme }

                    }
                };

            await _apiClient.AddLearnerData(ukprn, learnerData);

            return learnerData;
        }

        public async Task<LearnerDataRequest> AddLearnerData(string uln, long ukprn, List<CostDetails> costs)
        {
            var fixture = new Fixture();

            var onProgramme = fixture.Build<StubOnProgramme>()
                .With(x => x.StartDate, DateTime.UtcNow)
                .With(x => x.ExpectedEndDate, DateTime.UtcNow.AddYears(1))
                .With(x => x.AgreementId, "AG1")
                .With(x => x.LearnAimRef, "ZPROG001")
                .With(x => x.StandardCode, 57)
                .With(x => x.Costs, costs)
                .With(x => x.LearningSupport, fixture.Create<List<LearningSupport>>())
                .Create();


            var learnerData = new LearnerDataRequest
                {
                    ConsumerReference = fixture.Create<string>(),
                    Learner = fixture.Build<StubLearner>()
                    .With(x => x.Uln, uln)
                    .With(x => x.Email, $"{uln}@test.com")
                    .Create(),
                    Delivery = new StubDelivery
                    {
                        EnglishAndMaths = fixture.Create<List<StubEnglishAndMaths>>(),
                        OnProgramme = new[] { onProgramme }
                    }
                };

            await _apiClient.AddLearnerData(ukprn, learnerData);

            return learnerData;
        }

        public async Task<GetLearnerResponse> GetLearnersForProvider(long ukprn, int academicYear)
        {
            return await _apiClient.GetLearners(ukprn, academicYear);
        }

        public async Task<GetProviderRefDataResponse> GetProviderRefData(long ukprn)
        {
            return await _apiClient.GetProviderRefData(ukprn);
        }

        public async Task UpdateLearning(Guid apprenticeshipKey, Action<LearnerDataBuilder> configure)
        {
            var builder = new LearnerDataBuilder(_context.Get<TestData>());
            configure(builder);
            var request = builder.Build();
            var learnerKey = await ResolveLearnerKey(apprenticeshipKey);

            await _apiClient.UpdateLearning(Constants.UkPrn, learnerKey, request);
        }


        public async Task<UpdateLearnerRequest> UpdateLearning(Guid apprenticeshipKey, UpdateLearnerRequest request)
        {
            var learnerKey = await ResolveLearnerKey(apprenticeshipKey);

            await _apiClient.UpdateLearning(Constants.UkPrn, learnerKey, request);

            return request;
        }

        public async Task RemoveLearner(Guid apprenticeshipKey)
        {
            var learnerKey = await ResolveLearnerKey(apprenticeshipKey);

            await _apiClient.DeleteLearner(Constants.UkPrn, learnerKey);
        }

        private async Task<Guid> ResolveLearnerKey(Guid apprenticeshipKey)
        {
            Guid? learnerKey = null;

            await WaitHelper.WaitForIt(() =>
            {
                try
                {
                    learnerKey = _learningSqlClient.GetApprenticeship(apprenticeshipKey).LearnerKey;
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }, $"Unable to resolve learner key for apprenticeship learning key {apprenticeshipKey}");

            return learnerKey!.Value;
        }
    }
}
