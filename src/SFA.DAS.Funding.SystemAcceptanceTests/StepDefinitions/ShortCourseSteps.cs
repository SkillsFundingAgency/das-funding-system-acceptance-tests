using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using NUnit.Framework;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ShortCourseSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient)
{
    [When(@"SLD informs us of a short course for the learner starting on (.*)")]
    public async Task WhenTheProviderAddsAShortCourseForTheLearnerInTheCurrentAcademicYear(TokenisableDateTime startDate)
    {
        var testData = context.Get<TestData>();
        
        var endDate = startDate.Value.AddMonths(3);

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(startDate.Value)
            .WithEndDate(endDate)
            .Build();

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);

        testData.ShortCourseLearnerData = shortCourseRequest;
    }

    [Then(@"the basic short course earnings are generated")]
    public async Task ThenTheShortCourseIsSuccessfullyProcessed()
    {
        var testData = context.Get<TestData>();

        var expectedCourse = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();
        var expectedStartDate = expectedCourse.StartDate;
        var expectedEndDate = expectedCourse.ExpectedEndDate;

        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;
        }, "Failed to find short course earnings entity.");

        var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;

        var duration = (expectedEndDate - expectedStartDate).Days + 1;
        var daysToFirstPayment = (int)Math.Floor(duration * 0.3);
        var firstPaymentDate = expectedStartDate.AddDays(daysToFirstPayment);
        var secondPaymentDate = expectedEndDate;

        var expectedFirstPeriod = TableExtensions.Period[firstPaymentDate.ToString("MMMM")];
        var expectedFirstAcademicYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", firstPaymentDate));

        var expectedSecondPeriod = TableExtensions.Period[secondPaymentDate.ToString("MMMM")];
        var expectedSecondAcademicYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", secondPaymentDate));

        var firstInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedFirstPeriod && x.AcademicYear == expectedFirstAcademicYear);
        Assert.IsNotNull(firstInstalment, $"Could not find first instalment in period {expectedFirstPeriod} of AY {expectedFirstAcademicYear}");

        var secondInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedSecondPeriod && x.AcademicYear == expectedSecondAcademicYear);
        Assert.IsNotNull(secondInstalment, $"Could not find second instalment in period {expectedSecondPeriod} of AY {expectedSecondAcademicYear}");

        var totalPrice = earningsModel.Episodes.Single().CoursePrice;

        var expectedFirstAmount = Math.Round(totalPrice * 0.3m, 5);
        var expectedSecondAmount = Math.Round(totalPrice * 0.7m, 5);

        Assert.AreEqual((double)expectedFirstAmount, (double)firstInstalment.Amount, 0.01, "First instalment amount does not match exactly 30% of total price.");
        Assert.AreEqual((double)expectedSecondAmount, (double)secondInstalment.Amount, 0.01, "Second instalment amount does not match exactly 70% of total price.");
    }
}
