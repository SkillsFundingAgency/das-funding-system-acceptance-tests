using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders
{
    public class LearnerDataBuilder(TestData testData)
    {
        private bool _startDateSetExplicitly = false;

        private readonly UpdateLearnerRequest _request = new()
        {
            Delivery = new Delivery
            {
                OnProgramme = new List<OnProgramme> { new OnProgramme { AgreementId = "1", Care = new Care()}}
            },
            Learner = new LearnerRequestDetails
            {
                FirstName = testData.LearningCreatedEvent.FirstName,
                LastName = testData.LearningCreatedEvent.LastName,
                Dob = testData.LearningCreatedEvent.DateOfBirth
            }
        };

        public LearnerDataBuilder WithLearnersPersonalDetails(string firstName, string lastName, string? email)
        {
            _request.Learner.FirstName = firstName;
            _request.Learner.LastName = lastName;
            _request.Learner.Email = email;

            return this;
        }

        public LearnerDataBuilder WithDateOfBirth (DateTime dob)
        {
            _request.Learner.Dob = dob;

            return this;
        }

        public LearnerDataBuilder WithCostDetails(int? trainingPrice, int? epaoPrice, DateTime? fromDate)
        {
            _request.Delivery.OnProgramme.Latest().Costs.Add(new CostDetails
            {
                TrainingPrice = trainingPrice,
                EpaoPrice = epaoPrice,
                FromDate = fromDate
            });

            return this;
        }

        public LearnerDataBuilder WithLatestPeriodOfLearningHavingCost(int? trainingPrice, int? epaoPrice)
        {
            _request.Delivery.OnProgramme.Latest().Costs.First().TrainingPrice = trainingPrice;
            _request.Delivery.OnProgramme.Latest().Costs.First().EpaoPrice = epaoPrice;

            return this;
        }

        public LearnerDataBuilder WithEmptyCostDetails()
        {
            _request.Delivery.OnProgramme.Latest().Costs ??= new List<CostDetails>();

            return this;
        }

        public LearnerDataBuilder WithStartDate(DateTime startDate)
        {
            _request.Delivery.OnProgramme.Latest().StartDate = startDate;
            _startDateSetExplicitly = true;

            return this;
        }

        public LearnerDataBuilder WithExpectedEndDate(DateTime Date)
        {
            _request.Delivery.OnProgramme.Latest().ExpectedEndDate = Date;
            return this;
        }

        public LearnerDataBuilder WithCompletionDate(DateTime? completionDate)
        {
            _request.Delivery.OnProgramme.Latest().CompletionDate = completionDate;

            return this;
        }

        public LearnerDataBuilder WithPauseDate(DateTime? pauseDate)
        {
            _request.Delivery.OnProgramme.Latest().PauseDate = pauseDate;

            return this;
        }

        public LearnerDataBuilder WithWithdrawalDate(DateTime? withdrawalDate)
        {
            _request.Delivery.OnProgramme.Latest().WithdrawalDate = withdrawalDate;

            return this;
        }

        public LearnerDataBuilder WithEhcp (bool hasEhcp)
        {
            _request.Learner.HasEhcp = hasEhcp;

            return this;
        }

        public LearnerDataBuilder WithCareLeaver (bool careLeaver, bool employerConsent)
        {
            _request.Delivery.OnProgramme.Latest().Care.Careleaver = careLeaver;
            _request.Delivery.OnProgramme.Latest().Care.EmployerConsent = employerConsent;

            return this;
        }

        public LearnerDataBuilder WithOnProgrammeLearningSupport(DateTime startDate, DateTime endDate)
        {
            _request.Delivery.OnProgramme.Latest().LearningSupport.Add(new LearningSupport
            {
                StartDate = startDate,
                EndDate = endDate
            });
            return this;
        }

        public LearnerDataBuilder WithStandardCode(int standardCode)
        {
            _request.Delivery.OnProgramme.Latest().StandardCode = standardCode;

            return this;
        }

        public LearnerDataBuilder WithNoOnProgrammeLearningSupport()
        {
            _request.Delivery.OnProgramme.Latest().LearningSupport.Clear();
            return this;
        }

        public LearnerDataBuilder WithMathsAndEnglish(Func<EnglishAndMathsBuilder, EnglishAndMathsBuilder> configure)
        {
            var builder = new EnglishAndMathsBuilder();
            var course = configure(builder).Build();
            _request.Delivery.EnglishAndMaths.Add(course);
            return this;
        }

        public LearnerDataBuilder WithEnglishAndMaths(DateTime startDate, DateTime endDate, string course, decimal amount, string learnAimRef, DateTime? completionDate = null, DateTime? withdrawalDate = null, int? priorLearningPercentage = null)
        {

            var mathsAndEnglish = new EnglishAndMaths
            {
                StartDate = startDate,
                EndDate = endDate,
                Course = course,
                Amount = amount,
                CompletionDate = completionDate,
                WithdrawalDate = withdrawalDate,
                PriorLearningPercentage = priorLearningPercentage,
                LearnAimRef = learnAimRef
            };

            return WithEnglishAndMaths(mathsAndEnglish);
        }

        public LearnerDataBuilder WithEnglishAndMaths(EnglishAndMaths course)
        {
            _request.Delivery.EnglishAndMaths.Add(course);
            return this;
        }

        public LearnerDataBuilder WithEnglishAndMaths(IEnumerable<EnglishAndMaths> courses)
        {
            _request.Delivery.EnglishAndMaths.AddRange(courses);
            return this;
        }

        public LearnerDataBuilder WithNoMathsAndEnglish()
        {
            _request.Delivery.EnglishAndMaths.Clear();
            return this;
        }

        public LearnerDataBuilder WithAgreementId(string agreementId)
        {
            _request.Delivery.OnProgramme.Latest().AgreementId = agreementId;
            return this;
        }

        public LearnerDataBuilder WithReturnFromBreakInLearning(DateTime newLearningStartDate, bool correction = false, DateTime? newExpectedEndDate = null)
        {
            if(correction) _request.Delivery.OnProgramme.RemoveAt(1); //assume we are dealing with a single return being corrected for now

            _request.Delivery.OnProgramme.Latest().ActualEndDate = _request.Delivery.OnProgramme.Latest().PauseDate;
            _request.Delivery.OnProgramme.Add(new OnProgramme
            {
                AgreementId = _request.Delivery.OnProgramme.Latest().AgreementId,
                Care = new Care (),
                Costs = new List<CostDetails>
                {
                    new CostDetails
                    {
                        EpaoPrice = _request.Delivery.OnProgramme.Latest().Costs.First().EpaoPrice,
                        TrainingPrice = _request.Delivery.OnProgramme.Latest().Costs.First().TrainingPrice,
                        FromDate = newLearningStartDate
                    }
                },
                ExpectedEndDate = newExpectedEndDate ?? _request.Delivery.OnProgramme.Latest().ExpectedEndDate,
                LearningSupport = _request.Delivery.OnProgramme.Latest().LearningSupport,
                StandardCode = _request.Delivery.OnProgramme.Latest().StandardCode,
                StartDate = newLearningStartDate
            });

            return this;
        }

        public LearnerDataBuilder WithEnglishAndMAthsReturnFromBreakInLearning(DateTime newLearningStartDate, bool correction = false, DateTime? newExpectedEndDate = null)
        {
            if (correction) _request.Delivery.EnglishAndMaths.RemoveAt(1); //assume we are dealing with a single return being corrected for now

            _request.Delivery.EnglishAndMaths.Last().ActualEndDate = _request.Delivery.EnglishAndMaths.Last().PauseDate;
            
            _request.Delivery.EnglishAndMaths.Add(new EnglishAndMaths
            {
                Course = _request.Delivery.EnglishAndMaths.Last().Course,
                LearnAimRef = _request.Delivery.EnglishAndMaths.Last().LearnAimRef,
                StartDate = newLearningStartDate,
                EndDate = newExpectedEndDate ?? _request.Delivery.EnglishAndMaths.Last().EndDate,
                Amount = _request.Delivery.EnglishAndMaths.Last().Amount,
                LearningSupport = _request.Delivery.EnglishAndMaths.Last().LearningSupport,
            });

            return this;
        }

        public LearnerDataBuilder WithBreakInLearningReturnRemoved()
        {
            _request.Delivery.OnProgramme.RemoveAt(1); //assume we are dealing with a single return being removed for now

            return this;
        }

        public LearnerDataBuilder WithEntireBreakInLearningRemoved()
        {
            _request.Delivery.OnProgramme.RemoveAt(1); //assume we are dealing with a single return being removed for now
            _request.Delivery.OnProgramme.Latest().PauseDate = null;

            return this;
        }

        public UpdateLearnerRequest Build()
        {
            CalculateStartDate();
            return _request;
        }

        private void CalculateStartDate()
        {
            if (_startDateSetExplicitly) return;

            foreach (var onProgramme in _request.Delivery.OnProgramme)
            {
                onProgramme.StartDate = onProgramme.Costs.MinBy(x => x.FromDate)!.FromDate.GetValueOrDefault();
            }
        }
    }
}