using SFA.DAS.Funding.ApprenticeshipPayments.Types;

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

    public static void ShouldHaveCorrectPaymentsGenerated(this List<Payment> actual, List<(short AcademicYear, byte DeliveryPeriod, decimal Amount, short CollectionYear, byte CollectionPeriod)> expected, string paymentType = "OnProgramme")
    {
        var lowerBoundaryPeriod = expected.MinBy(x => x.AcademicYear + x.CollectionPeriod);
        var upperBoundaryPeriod = expected.MaxBy(x => x.AcademicYear + x.CollectionPeriod);

        actual.Should().NotContain(x => new Period(x.AcademicYear, x.CollectionPeriod).IsBefore(new Period(lowerBoundaryPeriod.AcademicYear, lowerBoundaryPeriod.CollectionPeriod)));
        actual.Should().NotContain(x => new Period(x.AcademicYear, x.CollectionPeriod).IsAfter(new Period(upperBoundaryPeriod.AcademicYear, upperBoundaryPeriod.CollectionPeriod)));

        actual.Count(x => x.PaymentType == paymentType).Should().Be(expected.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            actual[i].AcademicYear.Should().Be(expected[i].AcademicYear, $"Expected AcadmicYear #{i+1} to be {expected[i].AcademicYear} but found {actual[i].AcademicYear}");
            actual[i].DeliveryPeriod.Should().Be(expected[i].DeliveryPeriod, $"Expected DeliveryPeriod #{i+1} to be {expected[i].DeliveryPeriod} but found {actual[i].DeliveryPeriod}");
            actual[i].Amount.Should().Be(expected[i].Amount, $"Expected Amount #{i+1} to be {expected[i].Amount} but found {actual[i].Amount}");
            actual[i].CollectionYear.Should().Be(expected[i].CollectionYear, $"Expected CollectionYear #{i+1} to be {expected[i].CollectionYear} but found {actual[i].CollectionYear}");
            actual[i].CollectionPeriod.Should().Be(expected[i].CollectionPeriod, $"Expected CollectionPeriod #{i+1} to be {expected[i].CollectionPeriod} but found {actual[i].CollectionPeriod}");
        }
    }

    public static IEnumerable<DeliveryPeriod> FilterByOnProg(this IEnumerable<DeliveryPeriod> deliveryPeriods)
    {
        return deliveryPeriods.Where(x => x.InstalmentType == "OnProgramme");
    }
}