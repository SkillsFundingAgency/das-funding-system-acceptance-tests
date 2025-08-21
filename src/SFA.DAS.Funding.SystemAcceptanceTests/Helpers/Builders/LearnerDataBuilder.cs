using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders
{
    public class LearnerDataBuilder
    {
        private readonly UpdateLearnerRequest _request = new();

        public LearnerDataBuilder WithCompletionDate(DateTime? completionDate)
        {
            _request.Delivery.CompletionDate = completionDate;
            return this;
        }

        public LearnerDataBuilder WithOnProgrammeLearningSupport(DateTime startDate, DateTime endDate)
        {
            _request.Delivery.OnProgramme.LearningSupport.Add(new LearningSupport
            {
                StartDate = startDate,
                EndDate = endDate
            });
            return this;
        }

        public LearnerDataBuilder WithNoOnProgrammeLearningSupport()
        {
            _request.Delivery.OnProgramme.LearningSupport.Clear();
            return this;
        }

        public LearnerDataBuilder WithMathsAndEnglish(Func<MathsAndEnglishBuilder, MathsAndEnglishBuilder> configure)
        {
            var builder = new MathsAndEnglishBuilder();
            var course = configure(builder).Build();
            _request.Delivery.MathsAndEnglishCourses.Add(course);
            return this;
        }

        public LearnerDataBuilder WithMathsAndEnglish(DateTime startDate, DateTime endDate, string course, decimal amount, DateTime? completionDate = null, DateTime? withdrawalDate = null, int? priorLearningPercentage = null)
        {
            var mathsAndEnglish = new MathsAndEnglish
            {
                StartDate = startDate,
                PlannedEndDate = endDate,
                Course = course,
                Amount = amount,
                CompletionDate = completionDate,
                WithdrawalDate = withdrawalDate,
                PriorLearningPercentage = priorLearningPercentage
            };

            return WithMathsAndEnglish(mathsAndEnglish);
        }

        public LearnerDataBuilder WithMathsAndEnglish(MathsAndEnglish course)
        {
            _request.Delivery.MathsAndEnglishCourses.Add(course);
            return this;
        }

        public LearnerDataBuilder WithMathsAndEnglish(IEnumerable<MathsAndEnglish> courses)
        {
            _request.Delivery.MathsAndEnglishCourses.AddRange(courses);
            return this;
        }

        public UpdateLearnerRequest Build()
        {
            return _request;
        }
    }
}