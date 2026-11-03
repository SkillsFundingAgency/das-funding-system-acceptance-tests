using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Apprenticeship.Common;

[Binding]
public class UpdateStepDefinitions(
    ScenarioContext context,
    LearnerDataOuterApiHelper learnerDataOuterApiHelper,
    LearnerDataSqlClient learnerDataSqlClient,
    LearningSqlClient learningSqlClient,
    Helpers.Http.ApprovalsStubClient approvalsStubClient)
{

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

        testData.UpdateLearnerData = await learnerDataOuterApiHelper.UpdateLearning(testData.LearnerKey, learnerData);
    }

    [Given(@"SLD submit updated learners details \(with autoapprove\)")]
    [When(@"SLD submit updated learners details \(with autoapprove\)")]
    public async Task WhenSLDSubmitUpdatedLearnersDetailsWithAutoapprove()
    {
        await MockAutoApproveResponses();
        await WhenSLDSubmitUpdatedLearnersDetails();
    }

    private async Task MockAutoApproveResponses()
    {
        var testData = context.Get<TestData>();

        if (testData.LearnerDataBuilder == null)
        {
            throw new InvalidOperationException(
                "No learner data builder has been stored; cannot build or compare learner data for auto-approval");
        }

        var previous = GetPreviousSnapshot(testData);
        var currentRequest = testData.LearnerDataBuilder.Build();
        var current = ToSnapshot(currentRequest);

        var changes = new List<Helpers.Http.ApprovalsStubClient.ApprovalsChange>();

        if (!string.Equals(previous.Prices, current.Prices, StringComparison.Ordinal))
        {
            changes.Add(CreateChange("Prices", previous.Prices, current.Prices));
        }

        if (previous.ExpectedEndDate != current.ExpectedEndDate)
        {
            changes.Add(CreateChange("ExpectedEndDate", ToWireDate(previous.ExpectedEndDate), ToWireDate(current.ExpectedEndDate)));
        }

        if (!string.Equals(previous.PersonalDetails, current.PersonalDetails, StringComparison.Ordinal))
        {
            changes.Add(CreateChange("PersonalDetails", previous.PersonalDetails, current.PersonalDetails));
        }

        if (previous.DateOfBirth != current.DateOfBirth)
        {
            changes.Add(CreateChange("DateOfBirthChanged", ToWireDate(previous.DateOfBirth), ToWireDate(current.DateOfBirth)));
        }

        if (previous.StartDate != current.StartDate)
        {
            changes.Add(CreateChange("StartDate", ToWireDate(previous.StartDate), ToWireDate(current.StartDate)));
        }

        if (changes.Count == 0)
        {
            throw new InvalidOperationException("No supported changes detected for approvals auto-approval setup.");
        }

        await approvalsStubClient.SetUpAutoApproval(testData.LearningKey, changes.ToArray());
    }

    private static Snapshot GetPreviousSnapshot(TestData testData)
    {
        if (testData.UpdateLearnerData != null)
        {
            return ToSnapshot(testData.UpdateLearnerData);
        }

        if (testData.LearnerData != null)
        {
            return ToSnapshot(testData.LearnerData);
        }

        throw new InvalidOperationException("No previous learner data snapshot found on test data for approvals comparison.");
    }

    private static Snapshot ToSnapshot(UpdateLearnerRequest request)
    {
        var onProgramme = request.Delivery.OnProgramme
            .OrderByDescending(x => x.AimSequenceNumber)
            .ThenByDescending(x => x.StartDate)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Cannot compare learner data changes because no on-programme record exists.");

        return new Snapshot(
            StartDate: onProgramme.StartDate,
            ExpectedEndDate: onProgramme.ExpectedEndDate,
            DateOfBirth: request.Learner.Dob,
            PersonalDetails: $"{request.Learner.FirstName}|{request.Learner.LastName}|{request.Learner.Email}",
            Prices: FormatPrices(onProgramme.Costs));
    }

    private static Snapshot ToSnapshot(LearnerDataRequest request)
    {
        var onProgramme = request.Delivery.OnProgramme
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefault()
            ?? throw new InvalidOperationException("Cannot compare learner data changes because no on-programme record exists.");

        return new Snapshot(
            StartDate: onProgramme.StartDate.GetValueOrDefault(),
            ExpectedEndDate: onProgramme.ExpectedEndDate.GetValueOrDefault(),
            DateOfBirth: request.Learner.Dob.GetValueOrDefault(),
            PersonalDetails: $"{request.Learner.Firstname}|{request.Learner.Lastname}|{request.Learner.Email}",
            Prices: FormatPrices(onProgramme.Costs));
    }

    private static string FormatPrices(IEnumerable<CostDetails>? costs)
    {
        return string.Join(";",
            (costs ?? [])
            .OrderBy(x => x.FromDate)
            .Select(x => $"{ToWireDate(x.FromDate.GetValueOrDefault())}|{x.TrainingPrice}|{x.EpaoPrice}"));
    }

    private static Helpers.Http.ApprovalsStubClient.ApprovalsChange CreateChange(string changeType, string oldValue, string newValue)
    {
        return new Helpers.Http.ApprovalsStubClient.ApprovalsChange
        {
            ChangeType = changeType,
            Data = new Helpers.Http.ApprovalsStubClient.ApprovalsChangeData
            {
                Old = oldValue,
                New = newValue
            }
        };
    }

    private static string ToWireDate(DateTime date) => date.ToString("yyyy-MM-dd");

    private sealed record Snapshot(DateTime StartDate, DateTime ExpectedEndDate, DateTime DateOfBirth, string PersonalDetails, string Prices);
}
