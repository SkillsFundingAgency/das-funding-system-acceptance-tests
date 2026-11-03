namespace SFA.DAS.Funding.SystemAcceptanceTests;

internal static class TestDataExtensions
{
    internal static string GetLearnerRef(this TestData testData)
    {
        // Try both request models because different tests are set up via different learner data flows.
        return testData.LearnerData?.Learner?.LearnerRef
               ?? testData.UpdateLearnerData?.Learner?.LearnerRef
               ?? throw new InvalidOperationException("Cannot determine LearnerRef from TestData. Neither LearnerData nor UpdateLearnerData contains a learner reference.");
    }

    internal static DateTime ApprenticeshipStartDate(this TestData testData)
    {
        if (testData.CommitmentsApprenticeshipCreatedEvent != null)
        {
            return testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.GetValueOrDefault();
        }

        if(testData.LearnerData != null)
        {
            return testData.LearnerData.Delivery.OnProgramme.First().StartDate!.Value;
        }

        throw new InvalidOperationException("Cannot determine Apprenticeship Start Date from TestData");
    }
}
