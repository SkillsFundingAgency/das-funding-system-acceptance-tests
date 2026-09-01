namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

public static class Extensions
{
    public static void ShouldHaveCorrectFundingPeriods(this List<DeliveryPeriod> actual, List<(byte Period, short AcademicYear, byte Month)> expected)
    {
        var lowerBoundaryPeriod = expected.MinBy(x => x.AcademicYear + x.Period);
        var upperBoundaryPeriod = expected.MaxBy(x => x.AcademicYear + x.Period);

        actual.Should().NotContain(x => new Period(x.AcademicYear, x.Period).IsBefore(new Period(lowerBoundaryPeriod.AcademicYear, lowerBoundaryPeriod.Period)));
        actual.Should().NotContain(x => new Period(x.AcademicYear, x.Period).IsAfter(new Period(upperBoundaryPeriod.AcademicYear, upperBoundaryPeriod.Period)));

        actual.Count.Should().Be(expected.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            actual[i].Period.Should().Be(expected[i].Period, $"Expected period #{i} to be {expected[i].Period}/{expected[i].AcademicYear}");
            actual[i].AcademicYear.Should().Be(expected[i].AcademicYear, $"Expected period #{i} to be {expected[i].Period}/{expected[i].AcademicYear}");
            actual[i].CalendarMonth.Should().Be(expected[i].Month, $"Expected Calendar Month in period #{i} to be {expected[i].Month}");
        }
    }

    public static void ShouldHaveCorrectFundingLineType(this List<DeliveryPeriod> actual, string expected)
    {
        for (var i = 0; i < actual.Count; i++)
        {
           expected.Should().Be(actual[i].FundingLineType, $"Expected funding line type #{i} to be {expected} but found {actual[i].FundingLineType}");
        }
    }

    public static IEnumerable<DeliveryPeriod> FilterByOnProg(this IEnumerable<DeliveryPeriod> deliveryPeriods)
    {
        return deliveryPeriods.Where(x => x.InstalmentType == "OnProgramme");
    }

    public static IEnumerable<InstalmentModel> FilterByOnProg(this IEnumerable<InstalmentModel> instalments)
    {
        return instalments.Where(x => x.Type == "Regular");
    }

    public static void ShouldHaveCorrectFundingPeriods(this List<InstalmentModel> actual, List<(byte Period, short AcademicYear, byte Month)> expected)
    {
        var ordered = actual.OrderBy(x => x.AcademicYear).ThenBy(x => x.DeliveryPeriod).ToList();

        var lowerBoundaryPeriod = expected.MinBy(x => x.AcademicYear + x.Period);
        var upperBoundaryPeriod = expected.MaxBy(x => x.AcademicYear + x.Period);

        ordered.Should().NotContain(x => new Period(x.AcademicYear, x.DeliveryPeriod).IsBefore(new Period(lowerBoundaryPeriod.AcademicYear, lowerBoundaryPeriod.Period)));
        ordered.Should().NotContain(x => new Period(x.AcademicYear, x.DeliveryPeriod).IsAfter(new Period(upperBoundaryPeriod.AcademicYear, upperBoundaryPeriod.Period)));

        ordered.Count.Should().Be(expected.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            ordered[i].DeliveryPeriod.Should().Be(expected[i].Period, $"Expected period #{i} to be {expected[i].Period}/{expected[i].AcademicYear}");
            ordered[i].AcademicYear.Should().Be(expected[i].AcademicYear, $"Expected period #{i} to be {expected[i].Period}/{expected[i].AcademicYear}");
            ordered[i].CalendarMonth().Should().Be(expected[i].Month, $"Expected Calendar Month in period #{i} to be {expected[i].Month}");
        }
    }

    // Delivery period 1 is August (the first month of the academic year), running through to period 12 (July).
    public static byte CalendarMonth(this InstalmentModel instalment)
    {
        return instalment.DeliveryPeriod <= 5 ? (byte)(instalment.DeliveryPeriod + 7) : (byte)(instalment.DeliveryPeriod - 5);
    }
}