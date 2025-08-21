using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;

public class MathsAndEnglishBuilder
{
    private readonly MathsAndEnglish _course = new();

    public MathsAndEnglishBuilder WithCourseDetails(
        DateTime startDate,
        DateTime endDate,
        string course,
        decimal amount)
    {
        _course.Course = course;
        _course.StartDate = startDate;
        _course.PlannedEndDate = endDate;
        _course.Amount = amount;
        return this;
    }

    public MathsAndEnglishBuilder WithLearningSupport(DateTime start, DateTime end)
    {
        _course.LearningSupport.Add(new LearningSupport
        {
            StartDate = start,
            EndDate = end
        });
        return this;
    }

    public MathsAndEnglishBuilder WithPriorLearningPercentage(int? percentage)
    {
        _course.PriorLearningPercentage = percentage;
        return this;
    }

    public MathsAndEnglishBuilder WithWithdrawalDate(DateTime withdrawalDate)
    {
        _course.WithdrawalDate = withdrawalDate;
        return this;
    }

    public MathsAndEnglish Build() => _course;
}