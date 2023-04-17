using SFA.DAS.Funding.ApprenticeshipPayments.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

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

    public static void ShouldHaveCorrectFundingLineType(this List<DeliveryPeriod> actual, string expected)
    {
        for (var i = 0; i < actual.Count; i++)
        {
           expected.Should().Be(actual[i].FundingLineType, $"Expected funding line type #{i} to be {expected} but found {actual[i].FundingLineType}");
        }
    }

    public static void ShouldHaveCorrectPaymentsGenerated(this List<Payment> actual, List<(short AcademicYear, byte DeliveryPeriod, decimal Amount, short PaymentYear, byte PaymentPeriod)> expected)
    {
        actual.Count.Should().Be(expected.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            expected[i].AcademicYear.Should().Be(actual[i].AcademicYear, $"Expected AcadmicYear #{i+1} to be {expected[i].AcademicYear} but found {actual[i].AcademicYear}");
            expected[i].DeliveryPeriod.Should().Be(actual[i].DeliveryPeriod, $"Expected DeliveryPeriod #{i+1} to be {expected[i].DeliveryPeriod} but found {actual[i].DeliveryPeriod}");
            expected[i].Amount.Should().Be(actual[i].Amount, $"Expected Amount #{i+1} to be {expected[i].Amount} but found {actual[i].Amount}");
            expected[i].PaymentYear.Should().Be(actual[i].PaymentYear, $"Expected PaymentYear #{i+1} to be {expected[i].PaymentYear} but found {actual[i].PaymentYear}");
            expected[i].PaymentPeriod.Should().Be(actual[i].PaymentPeriod, $"Expected PaymentPeriod #{i+1} to be {expected[i].PaymentPeriod} but found {actual[i].PaymentPeriod}");
        }
    }

    public static void ShouldHaveCorrectPaymentsGeneratedIncludingRollup(this List<Payment> actual, List<(byte DeliveryPeriod, byte PaymentPeriod, decimal Amount)> expected)
    {
        actual.Count.Should().Be(expected.Count);

        for (var i = 0; i < expected.Count; i++)
        {
            expected[i].DeliveryPeriod.Should().Be(actual[i].DeliveryPeriod, $"Expected DeliveryPeriod #{i + 1} to be {expected[i].DeliveryPeriod} but found {actual[i].DeliveryPeriod}");
            expected[i].Amount.Should().Be(actual[i].Amount, $"Expected Amount #{i + 1} to be {expected[i].Amount} but found {actual[i].Amount}");
            expected[i].PaymentPeriod.Should().Be(actual[i].PaymentPeriod, $"Expected PaymentPeriod #{i + 1} to be {expected[i].PaymentPeriod} but found {actual[i].PaymentPeriod}");
        }
    }
}