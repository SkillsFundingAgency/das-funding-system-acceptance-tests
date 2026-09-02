using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class LearningSteps (ScenarioContext context, LearningSqlClient learningSqlClient, LearnerDataOuterApiHelper learnerDataOuterApiHelper)
{
    [Then("learner (.*) returned from get learners endpoint")]
    public async Task LearnerIsOrIsNotReturnedFromGetLearners(string isReturned)
    {
        var testData = context.Get<TestData>();

        var learners = await learnerDataOuterApiHelper.GetLearnersForProvider(Constants.UkPrn, Convert.ToInt32(TableExtensions.CalculateAcademicYear("0")));

        switch (isReturned)
        {
            case "is":
                Assert.IsTrue(learners.Learners.Any(l => l.Uln == testData.Uln), $"Expected learner with ULN {testData.Uln} to be returned, but it was not.");
                break;
            case "is not":
                Assert.IsFalse(learners.Learners.Any(l => l.Uln == testData.Uln), $"Expected learner with ULN {testData.Uln} to not be returned, but it was.");
                break;
            default:
                throw new ArgumentException($"Invalid value for isReturned: {isReturned}. Expected 'is' or 'is not'.");
        }
    }

    [Then("all approved and active learners for the provider are returned in the response")]
    public async Task AllApprovedLearnersForTheProviderAreReturned()
    {
        var testData = context.Get<TestData>();

        await WaitHelper.WaitForIt(() => learningSqlClient.GetApprovedLearners(Constants.UkPrn, Convert.ToInt16(TableExtensions.CalculateAcademicYear("0"))) != null, "Unable to find Learners for Ukprn");

        var expectedLearners = learningSqlClient.GetApprovedLearners(Constants.UkPrn, Convert.ToInt16(TableExtensions.CalculateAcademicYear("0")));

        Assert.IsNotNull(expectedLearners);

        var actualLearners = testData.LearnersOnService;

        var mismatches = actualLearners.Learners
            .Where(l1 => !expectedLearners.Any(l2 => l2.Uln == l1.Uln && l2.Key == l1.Key))
            .ToList();
        foreach (var m in mismatches)
        {
            Console.WriteLine($"Mismatch: ULN={m.Uln}, Key={m.Key}");
        }

        bool allExist = actualLearners.Learners
            .All(l1 => expectedLearners.Any(l2 => l2.Uln == l1.Uln && l2.Key == l1.Key));

        Assert.IsTrue(allExist, "Some learners in LearnerData outer response do not match with learners in learning db");

        Assert.AreEqual(expectedLearners.Count, actualLearners.Total, "Total count does not match");
    }

    [Then("all provider reference data are returned in the response")]
    public async Task AllProviderReferenceDataAreReturned()
    {
        var testData = context.Get<TestData>();
        var Ukprn = 10000028;

        var actualRefData = testData.ProviderRefData;

        if (actualRefData is null)
        {
            Assert.Fail("Provider reference data was null");
            return;
        }

        Assert.AreEqual(Ukprn, actualRefData.Ukprn, "Ukprn does not match");
        Assert.AreEqual("Main", actualRefData.Type, "Type does not match");
        Assert.AreEqual("Active", actualRefData.Status, "Status does not match");
    }

    [Then("the history of old learning is maintained")]
    public async Task HistoryOfOldLearningIsMaintained()
    {
        var testData = context.Get<TestData>();

        List<LearningHistoryModel> learningHistory = [];

        await WaitHelper.WaitForIt(() =>
        {
            learningHistory = learningSqlClient
                .GetApprenticeship(testData.LearningKey)
                .LearningHistory;

            return learningHistory.Count > 0;
        }, "Expected 1 or more LearningHistory records");

        var mostRecentHistory = learningHistory
            .OrderByDescending(x => x.CreatedOn)
            .First();

        Assert.That(
            mostRecentHistory.CreatedOn,
            Is.InRange(DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddSeconds(1))
        );
    }

    [Given("Approvals Apprenticeship Id is stored in ApprenticeshipEpisode table")]
    public async Task ApprovalsApprenticeshipIdIsStoredInApprenticeshipEpisodeTable()
    {
        var testData = context.Get<TestData>();

        var episode = learningSqlClient
                .GetApprenticeship(testData.LearningKey)
                .Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent);

        Assert.AreEqual(testData.CommitmentsApprenticeshipCreatedEvent?.ApprenticeshipId, episode.ApprovalsApprenticeshipId, "Approvals Apprenticeship Id do not match.");
    }

    [Then("store the apprenticeship, english and maths and learning support details in learning db in a draft state")]
    public void StoreTheApprenticeshipEnglishAndMathsIncentivesAndLearningSupportDetailsInLearningDbInADraftState()
    {
        var testData = context.Get<TestData>();

        testData.LearningKey = learningSqlClient.GetApprenticeshipByUln(testData.Uln).Key;

        var episode = learningSqlClient
                .GetApprenticeshipByUln(testData.Uln)
                .Episodes.GetEpisode(Constants.UkPrn, testData.LearnerData.Delivery.OnProgramme.First().StandardCode.ToString());

        var englishAndMaths = learningSqlClient.GetApprenticeshipByUln(testData.Uln)
            .EnglishAndMaths.First();

        var learningSupport = learningSqlClient.GetApprenticeshipByUln(testData.Uln)
            .LearningSupport.First();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(episode, "Expected episode to be present in the database");
            Assert.IsFalse(episode.isApproved, "Expected episode to be in a draft state");

            Assert.IsNotNull(englishAndMaths, "Expected English and Maths record to be present in the database");
            Assert.AreEqual(testData.LearnerData.Delivery.EnglishAndMaths.First().StartDate, englishAndMaths.StartDate, "Expected English and Maths StartDate to match");

            Assert.IsNotNull(learningSupport, "Expected Learning Support record to be present in the database");
            Assert.AreEqual(testData.LearnerData.Delivery.OnProgramme.First().LearningSupport.First().StartDate, learningSupport.StartDate, "Expected Learning Support StartDate to match");
        });
    }
}
