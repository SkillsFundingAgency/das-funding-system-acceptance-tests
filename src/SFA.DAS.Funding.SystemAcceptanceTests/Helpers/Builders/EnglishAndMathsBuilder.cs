using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;

public class EnglishAndMathsBuilder
{
    private readonly EnglishAndMaths _course = new();

    public EnglishAndMathsBuilder WithCourseDetails(
        DateTime startDate,
        DateTime endDate,
        string course,
        decimal amount)
    {
        _course.Course = course;
        _course.StartDate = startDate;
        _course.EndDate = endDate;
        _course.Amount = amount;
        return this;
    }

    public EnglishAndMathsBuilder WithLearningSupport(DateTime start, DateTime end)
    {
        _course.LearningSupport.Add(new LearningSupport
        {
            StartDate = start,
            EndDate = end
        });
        return this;
    }

    public EnglishAndMathsBuilder WithPriorLearningPercentage(int? percentage)
    {
        _course.PriorLearningPercentage = percentage;
        return this;
    }

    public EnglishAndMathsBuilder WithWithdrawalDate(DateTime withdrawalDate)
    {
        _course.WithdrawalDate = withdrawalDate;
        return this;
    }

    public EnglishAndMathsBuilder WithCompletionDate(DateTime completionDate)
    {
        _course.CompletionDate = completionDate;
        return this;
    }

    public EnglishAndMaths Build() => _course;
}