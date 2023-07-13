
namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public static class ShortExtensions
    {
        public static short ToStartingCalendarYear(this short academicYear)
        {
            return short.Parse($"20{short.Parse(academicYear.ToString()[..2])}");
        }
    }
}
