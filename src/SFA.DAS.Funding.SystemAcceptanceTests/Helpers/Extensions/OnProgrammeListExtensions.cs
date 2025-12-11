using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions
{
    public static class OnProgrammeListExtensions
    {
        public static LearnerDataOuterApiClient.OnProgramme Latest(this List<LearnerDataOuterApiClient.OnProgramme> onProgrammes)
        {
            return onProgrammes.OrderBy(op => op.StartDate).Last();
        }
    }
}
