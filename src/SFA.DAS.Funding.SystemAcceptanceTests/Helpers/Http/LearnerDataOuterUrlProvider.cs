namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

public class LearnerDataOuterUrlProvider
{
    private readonly bool _useLegacyLearnerDataOuterUrls;

    public LearnerDataOuterUrlProvider(bool useLegacyLearnerDataOuterUrls)
    {
        _useLegacyLearnerDataOuterUrls = useLegacyLearnerDataOuterUrls;
    }

    public string AddLearnerData(long ukprn, int academicYear, byte collectionPeriod)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/learners"
            : $"/learnerdata/providers/{ukprn}/apprenticeships?academicYear={academicYear}&collectionPeriod={collectionPeriod}";

    public string AddShortCourseLearnerData(long ukprn, int academicYear, byte collectionPeriod)
        => $"/learnerdata/providers/{ukprn}/shortCourses?academicYear={academicYear}&collectionPeriod={collectionPeriod}";

    public string AddShortCourseLearnerDataWithResponse(long ukprn, int academicYear, byte collectionPeriod)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/shortCourses"
            : $"/learnerdata/providers/{ukprn}/shortCourses?academicYear={academicYear}&collectionPeriod={collectionPeriod}";

    public string GetLearners(long ukprn, int academicYear)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/Learners/providers/{ukprn}/academicyears/{academicYear}/learners"
            : $"/learnerdata/providers/{ukprn}/apprenticeships/learners?academicYear={academicYear}";

    public string GetProviderRefData(long ukprn) => $"/learnerdata/reference-data/providers/{ukprn}";

    public string GetShortCourseLearnerApprovedUlns(long ukprn, int academicYear)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/academicyears/{academicYear}/shortCourses"
            : $"/learnerdata/providers/{ukprn}/shortCourses/learners?academicYear={academicYear}";

    public string UpdateShortCourseLearning(long ukprn, Guid learnerKey, int academicYear, byte collectionPeriod)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/shortCourses/{learnerKey}"
            : $"/learnerdata/providers/{ukprn}/shortCourses/{learnerKey}?academicYear={academicYear}&collectionPeriod={collectionPeriod}";

    public string GetShortCourseEarningsData(long ukprn, int academicYear, byte collectionPeriod)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/collectionPeriods/{academicYear}/{collectionPeriod}/shortCourses"
            : $"/learnerdata/providers/{ukprn}/shortCourses/earnings?academicYear={academicYear}&collectionPeriod={collectionPeriod}";

    public string UpdateLearning(long ukprn, Guid learnerKey, int academicYear, byte collectionPeriod)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/learning/{learnerKey}"
            : $"/learnerdata/providers/{ukprn}/apprenticeships/{learnerKey}?academicYear={academicYear}&collectionPeriod={collectionPeriod}";

    public string GetFm36Block(long ukprn, int academicYear, byte collectionPeriod, int? pageSize = null, int? pageNumber = null)
    {
        if (_useLegacyLearnerDataOuterUrls)
        {
            var url = $"/learnerdata/Learners/providers/{ukprn}/collectionPeriod/{academicYear}/{collectionPeriod}/fm36data";

            if (pageSize.HasValue && pageNumber.HasValue)
            {
                url += $"?page={pageNumber.Value}&pageSize={pageSize.Value}";
            }

            return url;
        }

        var newUrl = $"/learnerdata/providers/{ukprn}/fm36data?academicYear={academicYear}&collectionPeriod={collectionPeriod}";

        if (pageSize.HasValue && pageNumber.HasValue)
        {
            newUrl += $"&page={pageNumber.Value}&pageSize={pageSize.Value}";
        }

        return newUrl;
    }

    public string DeleteLearner(long ukprn, Guid learnerKey, int academicYear)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/learning/{learnerKey}"
            : $"/learnerdata/providers/{ukprn}/apprenticeships/{learnerKey}?academicYear={academicYear}";

    public string DeleteShortCourse(long ukprn, Guid learnerKey, int academicYear)
        => _useLegacyLearnerDataOuterUrls
            ? $"/learnerdata/providers/{ukprn}/shortCourses/{learnerKey}"
            : $"/learnerdata/providers/{ukprn}/shortCourses/{learnerKey}?academicYear={academicYear}";

    public string CallHealthCheck() => "/learnerdata/health";
}
