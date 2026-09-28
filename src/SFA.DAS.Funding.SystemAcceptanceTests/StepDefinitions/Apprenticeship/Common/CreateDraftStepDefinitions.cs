using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Data;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;
using LearningSupport = SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient.LearningSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship.Common;

[Binding]
public class CreateDraftStepDefinitions(
    ScenarioContext context, 
    LearnerDataOuterApiHelper learnerDataOuterApiHelper, 
    LearnerDataSqlClient learnerDataSqlClient, 
    LearningSqlClient learningSqlClient)
{

    [Given("SLD inform us of a learner with with 2 onprogramme deliveries")]
    public async Task GivenSldInformUsOfALearnerWithWith2OnprogrammeDeliveries()
    {
        var testData = context.Get<TestData>();

        var earliestStartDate = DateTime.UtcNow.Date;
        var earliestEndDate = earliestStartDate.AddYears(1);

        var firstOnProgrammeCosts = new List<CostDetails>
            {
                new CostDetails
                {
                    TrainingPrice = 12000,
                    EpaoPrice = 3000,
                    FromDate = earliestStartDate
                }
            };

        var learnerData = learnerDataOuterApiHelper.CreateLearnerDataRequest(
            testData.Uln,
            firstOnProgrammeCosts,
            earliestStartDate,
            earliestEndDate,
            614,
            new List<LearningSupport>(),
            new List<StubEnglishAndMaths>());

        var secondOnProgrammeStartDate = earliestStartDate.AddMonths(1);
        learnerData.Delivery.OnProgramme =
        [
            learnerData.Delivery.OnProgramme.Single(),
                new StubOnProgramme
                {
                    Care = new Care(),
                    StandardCode = 811,
                    AgreementId = "AG1",
                    LearnAimRef = "ZPROG001",
                    StartDate = secondOnProgrammeStartDate,
                    ExpectedEndDate = secondOnProgrammeStartDate.AddYears(1),
                    CompletionDate = null,
                    WithdrawalDate = null,
                    Costs = new List<CostDetails>
                    {
                        new CostDetails
                        {
                            TrainingPrice = 12000,
                            EpaoPrice = 3000,
                            FromDate = secondOnProgrammeStartDate
                        }
                    },
                    LearningSupport = new List<LearningSupport>(),
                    IsFlexiJob = false,
                    PercentageOfTrainingLeft = 0
                }
        ];

        await learnerDataOuterApiHelper.AddLearnerData(Constants.UkPrn, learnerData);

        testData.LearnerData = learnerData;
        context.Set(testData);
    }

    [Given("SLD inform us of a learner with apprenticeship, english and maths, incentives and learning support having start date (.*), expected end date (.*), standard code (.*?) and agreed price (.*)")]
    public async Task LearnerWithStartDateExpectedEndDateStandardCodeAndAgreedPrice(TokenisableDateTime startDate, TokenisableDateTime expectedEndDate, int standardCode, int agreedPrice)
    {
        var testData = context.Get<TestData>();

        var costDetails = new List<CostDetails>
            {
                new CostDetails
                {
                    TrainingPrice = (int)(agreedPrice*0.8),
                    EpaoPrice = (int)(agreedPrice*0.2),
                    FromDate = startDate.Value
                }
            };

        var learningSupport = new List<LearningSupport>
            {
                new LearningSupport
                {
                    StartDate = startDate.Value,
                    EndDate = expectedEndDate.Value
                }
            };

        testData.IsLearningSupportAdded = true;

        var englishAndMaths = new List<StubEnglishAndMaths>
            {
                new StubEnglishAndMaths
                {
                    Course = "English Foundation",
                    LearnAimRef = "12345678",
                    StartDate = startDate.Value,
                    EndDate = expectedEndDate.Value,
                    CompletionDate = null,
                    WithdrawalDate = null,
                    Amount = 1000.00m,
                    LearningSupport = learningSupport,
                    AimSequenceNumber = 2
                }
            };

        var learnerData = await learnerDataOuterApiHelper.AddLearnerData(testData.Uln, Constants.UkPrn, costDetails, startDate.Value, expectedEndDate.Value, standardCode, learningSupport, englishAndMaths);
        testData.LearnerData = learnerData;
        context.Set(testData);
    }

    [When("SLD inform us of a learner with empty costs array")]
    public async Task LearnerWithEmptyCostsArray()
    {
        var testData = context.Get<TestData>();
        var learnerData = await learnerDataOuterApiHelper.AddLearnerData(testData.Uln, Constants.UkPrn, new List<CostDetails>());
        testData.LearnerData = learnerData;
        context.Set(testData);
    }

    [When(@"SLD inform us of a new Learner")]
    public async Task WhenSldInformUsOfANewLearner()
    {
        var testData = context.Get<TestData>();
        var learnerData = await learnerDataOuterApiHelper.AddLearnerData(testData.Uln, 10005077);
        testData.LearnerData = learnerData;
        context.Set(testData);
    }

    [When(@"SLD submit a record where the Training code resolves to a learningType of ""(.*)"" in the Courses API")]
    [When(@"the record is resubmitted with a different Training code which resolves to ""(.*)""")]
    public async Task WhenLearningTypeIsSubmittedOrResubmitted(string learningType)
    {
        var testData = context.Get<TestData>();

        if (testData.LearnerData == null)
        {
            testData.LearnerData = await learnerDataOuterApiHelper.AddLearnerData(
                testData.Uln,
                Constants.UkPrn,
                LearningTypeHelper.Parse(learningType));
        }
        else
        {
            testData.LearnerData.Delivery.OnProgramme.First().StandardCode = LearningTypeHelper.GetStandardCode(learningType);
            await learnerDataOuterApiHelper.AddLearnerData(Constants.UkPrn, testData.LearnerData);
        }

        context.Set(testData);
    }

    [When(@"SLD inform us that the training provider has resubmitted the same learner( with price change)?")]
    public async Task WhenSldInformUsThatTheTrainingProviderHasResubmittedTheSameLearner(string withPriceChange)
    {
        var testData = context.Get<TestData>();

        if (testData.LearnerData == null)
        {
            throw new InvalidOperationException("No learner data has been prepared for resubmission");
        }

        if (!string.IsNullOrWhiteSpace(withPriceChange))
        {
            var onProgramme = testData.LearnerData.Delivery.OnProgramme.First();
            var latestCost = onProgramme.Costs.OrderByDescending(c => c.FromDate).First();

            latestCost.TrainingPrice = 6000;
            latestCost.EpaoPrice = 1500;
        }

        await learnerDataOuterApiHelper.AddLearnerData(Constants.UkPrn, testData.LearnerData);
        context.Set(testData);
    }

    [When("SLD inform us of a learner with training price (.*), epao as (.*) and fromDate (.*)")]
    public async Task LearnerWithTrainingPriceEpaoAsAndFromDateFrom_Date(string trainingPrice, string epao, string fromDate)
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


}
