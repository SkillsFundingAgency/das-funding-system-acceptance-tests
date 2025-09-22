using NUnit.Framework.Interfaces;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class LearnerDataSteps(ScenarioContext context, LearnerDataOuterApiHelper learnerDataOuterApiHelper, LearnerDataSqlClient learnerDataSqlClient)
    {
        [When(@"SLD inform us of a new Learner")]
        public async Task WhenSldInformUsOfANewLearner()
        {
            var testData = context.Get<TestData>();
            var learnerData = await learnerDataOuterApiHelper.AddLearnerData(testData.Uln, 10005077, 2425);
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

            await learnerDataOuterApiHelper.UpdateLearning(testData.LearningKey, learnerData);
        }

        [Given("SLD record on-programme cost as total price (.*) from date (.*)")]
        [When("SLD record on-programme cost as total price (.*) from date (.*)")]
        public void WhenSLDRecordOn_ProgrammeCostAsTotalPriceFromDateCurrentAY(int totalPrice, TokenisableDateTime fromDate)
        {
            var testData = context.Get<TestData>();

            var trainingPrice = totalPrice * 0.8;
            var epaoPrice = totalPrice * 0.2;

            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithCostDetails((int)trainingPrice , (int)epaoPrice, fromDate.Value);
        }

        [When("SLD record on-programme training price (.*) with epao as (.*) from date (.*)")]
        public void WhenSLDRecordOn_ProgrammeTrainingPriceWithEpaoAsNullFromDateCurrentAY(int trainingPrice, string? epaoPrice, TokenisableDateTime fromDate)
        {
            var testData = context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();

            int? epao = string.IsNullOrWhiteSpace(epaoPrice) || epaoPrice.Equals("null", StringComparison.OrdinalIgnoreCase)
                ? null
                : Convert.ToInt32(epaoPrice);

            learnerDataBuilder.WithCostDetails(trainingPrice, epao, fromDate.Value);
        }




        [Then(@"the learner's details are added to Learner Data db")]
        public async Task ThenTheLearnerIsAddedToLearnerData()
        {
            var testData = context.Get<TestData>();
            var uln = testData.Uln;

            await WaitHelper.WaitForIt(() => learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln)) != null, "Unable to find LearnerData for Uln");

            var data = learnerDataSqlClient.GetLearnerData(Convert.ToInt64(uln));

            Assert.IsNotNull(data);

            data.Should()
                .BeEquivalentTo(testData.LearnerData,
                    options => options
                        .Excluding(x => x.LearnerEmail)
                        .Excluding(x => x.DateOfBirth)
                        .Excluding(x => x.StartDate)
                        .Excluding(x => x.PlannedEndDate)
                    );

            data.Email.Should().Be(testData.LearnerData.LearnerEmail);
            data.DoB.Date.Should().Be(testData.LearnerData.DateOfBirth.Date);
            data.StartDate.Date.Should().Be(testData.LearnerData.StartDate.Date);
            data.PlannedEndDate.Date.Should().Be(testData.LearnerData.PlannedEndDate.Date);
        }
    }
}
