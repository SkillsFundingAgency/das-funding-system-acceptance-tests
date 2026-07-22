namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public class LearnerDataOuterUrlProvider
{
    private readonly bool _useLegacyLearnerDataOuterUrls;

    public LearnerDataOuterUrlProvider(bool useLegacyLearnerDataOuterUrls)
    {
        _useLegacyLearnerDataOuterUrls = useLegacyLearnerDataOuterUrls;
    }

    public string AddLearnerData(long ukprn)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/learners"
            : "TODO new url";

    public string AddShortCourseLearnerData(long ukprn, int academicYear, byte collectionPeriod)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/shortCourses?academicYear={academicYear}&collectionPeriod={collectionPeriod}"
            : "TODO new url";

    public string AddShortCourseLearnerDataWithResponse(long ukprn)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/shortCourses"
            : "TODO new url";

    public string GetLearners(long ukprn, int academicYear)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/Learners/providers/{ukprn}/academicyears/{academicYear}/learners"
            : "TODO new url";

    public string GetProviderRefData(long ukprn)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/reference-data/providers/{ukprn}"
            : "TODO new url";

    public string GetShortCourseLearnerApprovedUlns(long ukprn, int academicYear)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/academicyears/{academicYear}/shortCourses"
            : "TODO new url";

    public string UpdateShortCourseLearning(long ukprn, Guid learnerKey)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/shortCourses/{learnerKey}"
            : "TODO new url";

    public string GetShortCourseEarningsData(long ukprn, int collectionYear, byte collectionPeriod)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/collectionPeriods/{collectionYear}/{collectionPeriod}/shortCourses"
            : "TODO new url";

    public string UpdateLearning(long ukprn, Guid learnerKey)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/learning/{learnerKey}"
            : "TODO new url";

    public string GetFm36Block(long ukprn, int collectionYear, byte collectionPeriod, int? pageSize = null, int? pageNumber = null)
    {
        if (_useLegacyLearnerDataOuterUrls)
        {
            var url = $"/learnerdata/Learners/providers/{ukprn}/collectionPeriod/{collectionYear}/{collectionPeriod}/fm36data";

            if (pageSize.HasValue && pageNumber.HasValue)
            {
                url += $"?page={pageNumber.Value}&pageSize={pageSize.Value}";
            }

            return url;
        }

        return "TODO new url";
    }

    public string DeleteLearner(long ukprn, Guid learnerKey)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/learning/{learnerKey}"
            : "TODO new url";

    public string DeleteShortCourse(long ukprn, Guid learnerKey)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/shortCourses/{learnerKey}"
            : "TODO new url";

    public string CallHealthCheck()
        => _useLegacyLearnerDataOuterUrls
            ? "/learnerdata/health"
            : "TODO new url";
}
