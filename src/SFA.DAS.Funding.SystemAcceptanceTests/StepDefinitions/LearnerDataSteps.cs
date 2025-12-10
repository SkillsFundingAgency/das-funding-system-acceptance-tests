using System.Text.Json;
using NUnit.Framework.Interfaces;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class LearnerDataSteps(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, LearnerDataSqlClient learnerDataSqlClient)
    {
        [When(@"SLD inform us of a new Learner")]
        public async Task WhenSldInformUsOfANewLearner()
        {
            var testData = context.Get<TestData>();
            var learnerData = await learnerDataOuterApiHelper.AddLearnerData(testData.Uln, 10005077);
            testData.LearnerData = learnerData;
            context.Set(testData);
        }

        [When("SLD inform us of a learner with empty costs array")]
        public async Task LearnerWithEmptyCostsArray()
        {
            var testData = context.Get<TestData>();
            var learnerData = await learnerDataOuterApiHelper.AddLearnerData(testData.Uln, Constants.UkPrn, new List<CostDetails> ());
            testData.LearnerData = learnerData;
            context.Set(testData);
        }

        [When("SLD inform us of a learner with training price (.*), epao as (.*) and fromDate (.*)")]
        public async Task LearnerWithTrainingPriceEpaoAsAndFromDateFrom_Date(string trainingPrice, string epao, string   fromDate)
        {
            var testData = context.Get<TestData>();
            var learnerData = await learnerDataOuterApiHelper.AddLearnerData(
                testData.Uln, 
                Constants.UkPrn, 
                new List<CostDetails>
                {
                    new CostDetails
                    {
                        TrainingPrice = trainingPrice == "null" ? null : int.Parse(trainingPrice),
                        EpaoPrice = epao == "null" ? null : int.Parse(epao),
                        FromDate = fromDate == "null" ? null : TokenisableDateTime.FromString(fromDate).Value

                    }
                });
            testData.LearnerData = learnerData;
            context.Set(testData);
        }

        [When("SLD want to know the learners already on Apprenticeship service for a provider")]
        public async Task SLDWantToKnowTheLearnersAlreadyOnApprenticeshipServiceForAProvider()
        {
            var testData = context.Get<TestData>();
            var learnerData = await learnerDataOuterApiHelper.GetLearnersForProvider(Constants.UkPrn, Convert.ToInt32(TableExtensions.CalculateAcademicYear("0")));

            testData.LearnersOnService = learnerData;
            context.Set(testData);
        }

        [Given(@"SLD submit updated learners details")]
        [When(@"SLD submit updated learners details")]
        public async Task WhenSLDSubmitUpdatedLearnersDetails()
        {
            var testData = context.Get<TestData>();

            if (testData.LearnerDataBuilder == null)
            {
                throw new InvalidOperationException(
                    "No learner data builder has been stored; cannot build or submit learner data");
            }

            var learnerData = testData.LearnerDataBuilder.Build();

            var debugPayload = JsonSerializer.Serialize(learnerData);

            await learnerDataOuterApiHelper.UpdateLearning(testData.LearningKey, learnerData);
        }

        [Given("SLD record on-programme cost as total price (.*) from date (.*) to date (.*)")]
        [When("SLD record on-programme cost as total price (.*) from date (.*) to date (.*)")]
        public void SLDRecordOnProgrammeCostFromDate(int totalPrice, TokenisableDateTime fromDate, TokenisableDateTime toDate)
        {
            var testData = context.Get<TestData>();

            var trainingPrice = totalPrice * 0.8;
            var epaoPrice = totalPrice * 0.2;

            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithCostDetails((int)trainingPrice , (int)epaoPrice, fromDate.Value);

            learnerDataBuilder.WithExpectedEndDate(toDate.Value);

            learnerDataBuilder.WithStandardCode(Convert.ToInt32(testData.CommitmentsApprenticeshipCreatedEvent.TrainingCode));
        }

        [When("SLD record latest on-programme cost as total price (.*)")]
        public void SLDRecordOnProgrammeCostFromDate(int totalPrice)
        {
            var testData = context.Get<TestData>();

            var trainingPrice = totalPrice * 0.8;
            var epaoPrice = totalPrice * 0.2;

            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithLatestPeriodOfLearningHavingCost((int)trainingPrice, (int)epaoPrice);
        }

        [Given("SLD record on-programme cost as total price (.*) from date (.*) with duration (.*)")]
        [When("SLD record on-programme cost as total price (.*) from date (.*) with duration (.*)")]
        public void SLDRecordOnProgrammeCostFromDateWithDuration(int totalPrice, TokenisableDateTime fromDate, int duration)
        {
            var testData = context.Get<TestData>();

            var plannedEndDate = fromDate.Value.AddDays(duration - 1);

            var trainingPrice = totalPrice * 0.8;
            var epaoPrice = totalPrice * 0.2;

            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithCostDetails((int)trainingPrice, (int)epaoPrice, fromDate.Value);

            learnerDataBuilder.WithExpectedEndDate(plannedEndDate);
        }

        [When("SLD record on-programme training price (.*) with epao as (.*) from date (.*) to date (.*)")]
        public void SLDRecordOnProgrammeTrainingPriceAndEpaoFromDate(string trainingPrice, string epaoPrice, string fromDate, TokenisableDateTime toDate)
        {
            var testData = context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            int? epao = string.IsNullOrWhiteSpace(epaoPrice) || epaoPrice.Equals("null", StringComparison.OrdinalIgnoreCase)
                ? null
                : Convert.ToInt32(epaoPrice);

            int? tp = string.IsNullOrWhiteSpace(trainingPrice) || trainingPrice.Equals("null", StringComparison.OrdinalIgnoreCase)
                ? null
                : Convert.ToInt32(trainingPrice);

            TokenisableDateTime? fd = string.IsNullOrWhiteSpace(fromDate) || fromDate.Equals("null", StringComparison.OrdinalIgnoreCase)
                ? null
                : TokenisableDateTime.FromString(fromDate);

            learnerDataBuilder.WithCostDetails(tp, epao, fd == null? null : fd.Value);

            learnerDataBuilder.WithExpectedEndDate(toDate.Value);

            learnerDataBuilder.WithStandardCode(Convert.ToInt32(testData.CommitmentsApprenticeshipCreatedEvent.TrainingCode));
        }

        [When("SLD record an empty on-programme costs array")]
        public void SLDRecordEmptyOnProgCostsArray()
        {
            var testData = context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithEmptyCostDetails();
        }

        [When("SLD record expected end date (.*)")]
        public void SLDRecordExpectedEndDate(TokenisableDateTime plannedEndDate)
        {
            var testData = context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithExpectedEndDate(plannedEndDate.Value);
        }

        [When("SLD record standard code as (.*)")]
        public void WhenSLDRecordStandardCodeAs(int standardCode)
        {
            var testData = context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithStandardCode(standardCode);
        }


        [When("SLD record on-prog start date as (.*)")]
        public void WhenSLDRecordOn_ProgStartDateAsPreviousAY(TokenisableDateTime startDate)
        {
            var testData = context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            learnerDataBuilder.WithStartDate(startDate.Value);
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
}
