using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Data;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class LearnerDataSteps(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, LearnerDataSqlClient learnerDataSqlClient, LearningSqlClient learningSqlClient)
{

    [Then("only inform Approvals of the course with the earliest start date")]
    public async Task ThenOnlyInformApprovalsOfTheCourseWithTheEarliestStartDate()
    {
        var testData = context.Get<TestData>();
        var expectedStandardCode = testData.LearnerData!.Delivery.OnProgramme
            .OrderBy(x => x.StartDate)
            .First().StandardCode;

        await WaitHelper.WaitForIt(() =>
        {
            var publishedEvent = LearnerDataEventHandler.GetMessage(x => x.ULN == long.Parse(testData.Uln));
            if (publishedEvent == null)
            {
                return false;
            }

            Assert.AreEqual(expectedStandardCode, publishedEvent.StandardCode,
                $"Expected StandardCode {expectedStandardCode} from earliest start date delivery but found {publishedEvent.StandardCode}");


            return true;
        }, "Failed to find published LearnerDataEvent for the learner.");
    }

    [Then("the progression learning with training code (.*) and start date (.*) is added to Learner Data db")]
    public async Task ProgressionLearningIsAddedToLearnerDataDb(int trainingCode, TokenisableDateTime startDate)
    {
        var testData = context.Get<TestData>();
        var uln = testData.Uln;

        await WaitHelper.WaitForIt(() => learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln)) != null, "Unable to find LearnerData for Uln");

        var data = learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln));

        Assert.IsNotNull(data);

        data = learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln));

        data.TrainingCode.Should().Be(testData.UpdateLearnerData.Delivery.OnProgramme.OrderByDescending(x => x.StartDate).FirstOrDefault()?.StandardCode);
        data.StartDate.Date.Should().Be(testData.UpdateLearnerData.Delivery.OnProgramme.OrderByDescending(x => x.StartDate).FirstOrDefault()?.StartDate!.Date);
    }

    [Then(@"we have stored the learningType of ""(.*)"" for that learning")]
    public async Task ThenWeHaveStoredTheLearningTypeForThatLearning(string expectedLearningType)
    {
        var testData = context.Get<TestData>();
        var expected = LearningTypeHelper.Parse(expectedLearningType);

        await WaitHelper.WaitForIt(
            () => learningSqlClient.GetApprenticeshipByUln(testData.Uln) != null,
            $"Unable to find apprenticeship learning for ULN {testData.Uln}");

        var actualLearningType = learningSqlClient.GetApprenticeshipLearningTypeByUln(testData.Uln);

        Assert.AreEqual((byte)expected, actualLearningType,
            $"Expected learningType {expected} but got {(LearningType)actualLearningType}");
    }

    [When("SLD want to know the learners already on Apprenticeship service for a provider")]
    public async Task SLDWantToKnowTheLearnersAlreadyOnApprenticeshipServiceForAProvider()
    {
        var testData = context.Get<TestData>();
        var learnerData = await learnerDataOuterApiHelper.GetLearnersForProvider(Constants.UkPrn, Convert.ToInt32(TableExtensions.CalculateAcademicYear("0")));

        testData.LearnersOnService = learnerData;
        context.Set(testData);
    }

    [When("SLD want to know the provider reference data for a provider")]
    public async Task SLDWantToKnowTheProviderReferenceDataForAProvider()
    {
        var testData = context.Get<TestData>();
        var Ukprn = 10000028;
        var providerRefData = await learnerDataOuterApiHelper.GetProviderRefData(Ukprn);

        testData.ProviderRefData = providerRefData;
        context.Set(testData);
    }

    [Then(@"the learner's details are added to Learner Data db")]
    public async Task ThenTheLearnerIsAddedToLearnerData()
    {
        var testData = context.Get<TestData>();
        var uln = testData.Uln;

        await WaitHelper.WaitForIt(() => learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln)) != null, "Unable to find LearnerData for Uln");

        var data = learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln));

        Assert.IsNotNull(data);

        data.Email.Should().Be(testData.LearnerData!.Learner.Email);
        data.DoB.Date.Should().Be(testData.LearnerData.Learner.Dob!.Value.Date);
        data.StartDate.Date.Should().Be(testData.LearnerData.Delivery.OnProgramme.First().StartDate!.Value.Date);
        data.PlannedEndDate.Date.Should().Be(testData.LearnerData.Delivery.OnProgramme.First().ExpectedEndDate!.Value.Date);
    }

    [Then("treat Training price as (.*), EPAO price as (.*) and fromDate as Start Date")]
    public async Task TreatTrainingPriceAsEPAOPriceAsAndFromDateAs(int? trainingPrice, int? epaoPrice)
    {
        var testData = context.Get<TestData>();
        var uln = testData.Uln;

        await WaitHelper.WaitForIt(() => learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln)) != null, "Unable to find LearnerData for Uln");

        var data = learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln));

        Assert.IsNotNull(data);

        data.StartDate.Should().Be(testData.LearnerData.Delivery.OnProgramme.First().StartDate!.Value.Date);
        data.TrainingPrice.Should().Be(trainingPrice);
        data.EpaoPrice.Should().Be(epaoPrice);
    }

}