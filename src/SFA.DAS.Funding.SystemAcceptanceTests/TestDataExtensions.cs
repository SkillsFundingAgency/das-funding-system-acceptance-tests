namespace SFA.DAS.Funding.SystemAcceptanceTests;

internal static class TestDataExtensions
{
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
