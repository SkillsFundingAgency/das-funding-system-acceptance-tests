using SFA.DAS.CommitmentsV2.Messages.Events;
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

        public async Task<LearnerDataRequest> AddLearnerData(long ukprn, LearnerDataRequest learnerData)
        {
            await _apiClient.AddLearnerData(ukprn, learnerData);
            return learnerData;
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
                        EnglishAndMaths = new List<StubEnglishAndMaths>
                        {
                            fixture.Build<StubEnglishAndMaths>()
                            .With(x => x.LearnAimRef, "E&M")
                            .Create(),
                        },
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
                        EnglishAndMaths = new List<StubEnglishAndMaths>
                        {
                            fixture.Build<StubEnglishAndMaths>()
                            .With(x => x.LearnAimRef, "E&M")
                            .Create(),
                        },
                        OnProgramme = new[] { onProgramme }
                    }
                };

            await _apiClient.AddLearnerData(ukprn, learnerData);

            return learnerData;
        }

        public async Task<LearnerDataRequest> AddLearnerData(string uln, long ukprn, List<CostDetails> costs, DateTime startDate, DateTime expectedEndDate, 
            int standardCode, List<LearningSupport> learningSupports, List<StubEnglishAndMaths> englishAndMaths)
        {
            var learnerData = CreateLearnerDataRequest(uln, costs, startDate, expectedEndDate, standardCode, learningSupports, englishAndMaths);

            await _apiClient.AddLearnerData(ukprn, learnerData);

            return learnerData;
        }

        public LearnerDataRequest CreateLearnerDataRequest(string uln, List<CostDetails> costs, DateTime startDate, DateTime expectedEndDate,
            int standardCode, List<LearningSupport> learningSupports, List<StubEnglishAndMaths> englishAndMaths)
        {
            var fixture = new Fixture();

            var onProgramme = fixture.Build<StubOnProgramme>()
                .With(x => x.StartDate, startDate)
                .With(x => x.ExpectedEndDate, expectedEndDate)
                .With(x => x.AgreementId, "AG1")
                .With(x => x.LearnAimRef, "ZPROG001")
                .With(x => x.StandardCode, standardCode)
                .With(x => x.CompletionDate, (DateTime?)null)
                .With(x => x.WithdrawalDate, (DateTime?)null)
                .With(x => x.Costs, costs)
                .With(x => x.LearningSupport, learningSupports)
                .Create();


            var learnerData = new LearnerDataRequest
            {
                ConsumerReference = fixture.Create<string>(),
                Learner = fixture.Build<StubLearner>()
                    .With(x => x.Uln, uln)
                    .With(x => x.Email, $"{uln}@test.com")
                    .With(x => x.Dob, startDate.AddYears(-17))
                    .Create(),
                Delivery = new StubDelivery
                {
                    EnglishAndMaths = englishAndMaths,
                    OnProgramme = new[] { onProgramme }
                }
            };

            return learnerData;
        }

        public async Task<LearnerDataRequest> AddLearnerData(string uln, long ukprn, LearningType learningType)
        {
            var fixture = new Fixture();

            var standardCode = learningType switch
            {
                LearningType.Apprenticeship => 614,
                LearningType.FoundationApprenticeship => 811,
                _ => throw new ArgumentOutOfRangeException(nameof(learningType), learningType, "Unsupported learningType")
            };

            var onProgramme = fixture.Build<StubOnProgramme>()
                .With(x => x.StartDate, DateTime.UtcNow)
                        .With(x => x.ExpectedEndDate, DateTime.UtcNow.AddYears(1))
                        .With(x => x.AgreementId, "AG1")
                        .With(x => x.StandardCode, 57)
                        .With(x => x.Costs, new List<CostDetails> { fixture.Create<CostDetails>() })
                        .With(x => x.LearningSupport, fixture.Create<List<LearningSupport>>())
                        .With(x => x.StandardCode, standardCode)
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
                    EnglishAndMaths = new List<StubEnglishAndMaths>
                        {
                            fixture.Build<StubEnglishAndMaths>()
                            .With(x => x.LearnAimRef, "E&M")
                            .Create(),
                        },
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

        public async Task UpdateLearning(Guid learningKey, Action<LearnerDataBuilder> configure)
        {
            var builder = new LearnerDataBuilder(_context.Get<TestData>());
            configure(builder);
            var request = builder.Build();

            await _apiClient.UpdateLearning(Constants.UkPrn, learningKey, request);
        }


        public async Task<UpdateLearnerRequest> UpdateLearning(Guid learningKey, UpdateLearnerRequest request)
        {
            await _apiClient.UpdateLearning(Constants.UkPrn, learningKey, request);

            return request;
        }

        public async Task RemoveLearner(Guid learningKey, short? academicYearOverride = null)
        {
            await _apiClient.DeleteLearner(Constants.UkPrn, learningKey, academicYearOverride);
        }
    }
}
