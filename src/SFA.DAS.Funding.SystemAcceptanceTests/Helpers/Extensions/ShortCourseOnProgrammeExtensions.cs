using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.Requests;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions
{
    public static class ShortCourseOnProgrammeExtensions
    {
        public static ShortCourseOnProgramme Latest(this List<ShortCourseOnProgramme> onProgrammes)
        {
            return onProgrammes.OrderBy(x => x.StartDate).Last();
        }
    }
}
