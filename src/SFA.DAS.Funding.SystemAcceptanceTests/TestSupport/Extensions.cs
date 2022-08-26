namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class Extensions
{
    public static void ShouldHaveCorrectFundingPeriods(this List<DeliveryPeriod> actual, List<(byte Period, short AcademicYear, byte Month)> expected)
    {
        actual.Count.Should().Be(expected.Count);
        
        for (var i = 0; i < expected.Count; i++)
        {
            actual[i].Period.Should().Be(expected[i].Period, $"Expected period #{i} to be {expected[i].Period}/{expected[i].AcademicYear}");
            actual[i].AcademicYear.Should().Be(expected[i].AcademicYear, $"Expected period #{i} to be {expected[i].Period}/{expected[i].AcademicYear}");
            actual[i].CalendarMonth.Should().Be(expected[i].Month, $"Expected Calendar Month in period #{i} to be {expected[i].Month}");
        }
    }
}