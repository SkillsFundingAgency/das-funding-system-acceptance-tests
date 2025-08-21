using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions
{
    internal static class TestDataExtensions
    {
        internal static LearnerDataBuilder GetLearnerDataBuilder(this TestData testData)
        {
            return testData.LearnerDataBuilder ?? (testData.LearnerDataBuilder = new LearnerDataBuilder());
        }
    }
}
