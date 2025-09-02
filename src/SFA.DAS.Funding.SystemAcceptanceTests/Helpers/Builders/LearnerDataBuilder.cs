using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders
{
    public class LearnerDataBuilder
    {
        private readonly UpdateLearnerRequest _request;

        public LearnerDataBuilder()
        {
            _request = new UpdateLearnerRequest
            {
                Delivery = new List<UpdateLearnerRequestDeliveryDetails>
            {
                new UpdateLearnerRequestDeliveryDetails()
            }
            };
        }

        public LearnerDataBuilder WithCompletionDate(DateTime? completionDate)
        {
            _request.Delivery.First().OnProgramme.CompletionDate = completionDate;

            return this;
        }

        public LearnerDataBuilder WithOnProgrammeLearningSupport(DateTime startDate, DateTime endDate)
        {
            _request.Delivery.First().OnProgramme.LearningSupport.Add(new LearningSupport
            {
                StartDate = startDate,
                EndDate = endDate
            });
            return this;
        }

        public LearnerDataBuilder WithNoOnProgrammeLearningSupport()
        {
            _request.Delivery.First().OnProgramme.LearningSupport.Clear();
            return this;
        }

        public LearnerDataBuilder WithMathsAndEnglish(Func<EnglishAndMathsBuilder, EnglishAndMathsBuilder> configure)
        {
            var builder = new EnglishAndMathsBuilder();
            var course = configure(builder).Build();
            _request.Delivery.First().EnglishAndMaths.Add(course);
            return this;
        }

        public LearnerDataBuilder WithEnglishAndMaths(DateTime startDate, DateTime endDate, string course, decimal amount, DateTime? completionDate = null, DateTime? withdrawalDate = null, int? priorLearningPercentage = null)
        {
            var mathsAndEnglish = new EnglishAndMaths
            {
                StartDate = startDate,
                EndDate = endDate,
                Course = course,
                Amount = amount,
                CompletionDate = completionDate,
                WithdrawalDate = withdrawalDate,
                PriorLearningPercentage = priorLearningPercentage
            };

            return WithEnglishAndMaths(mathsAndEnglish);
        }

        public LearnerDataBuilder WithEnglishAndMaths(EnglishAndMaths course)
        {
            _request.Delivery.First().EnglishAndMaths.Add(course);
            return this;
        }

        public LearnerDataBuilder WithEnglishAndMaths(IEnumerable<EnglishAndMaths> courses)
        {
            _request.Delivery.First().EnglishAndMaths.AddRange(courses);
            return this;
        }

        public LearnerDataBuilder WithNoMathsAndEnglish()
        {
            _request.Delivery.First().EnglishAndMaths.Clear();
            return this;
        }

        public UpdateLearnerRequest Build()
        {
            return _request;
        }
    }
}