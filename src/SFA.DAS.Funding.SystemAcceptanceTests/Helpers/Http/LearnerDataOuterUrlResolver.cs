namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http
{
    public class LearnerDataOuterUrlResolver
    {
        private readonly bool _useLegacyUrlFormats;

        public LearnerDataOuterUrlResolver(bool useLegacyUrlFormats)
        {
            _useLegacyUrlFormats = useLegacyUrlFormats;
        }

        public string AddLearnerData(long ukprn, short? academicYear = null, byte? collectionPeriod = null)
        {
            if (_useLegacyUrlFormats) return $"/learnerdata/providers/{ukprn}/learners";
            

            var url = $"/learnerdata/providers/{ukprn}/apprenticeships";
            var query = new List<string>();

            if (academicYear.HasValue) query.Add($"academicYear={academicYear.Value}");
            if (collectionPeriod.HasValue) query.Add($"collectionPeriod={collectionPeriod.Value}");
            if (query.Count > 0) url += $"?{string.Join("&", query)}";

            return url;
        }

        public string AddShortCourseLearnerData(long ukprn, short? academicYear = null, byte? collectionPeriod = null)
        {
            if (_useLegacyUrlFormats) return $"/learnerdata/providers/{ukprn}/shortCourses";

            var url = $"/learnerdata/providers/{ukprn}/shortCourses";
            var query = new List<string>();

            if (academicYear.HasValue) query.Add($"academicYear={academicYear.Value}");
            if (collectionPeriod.HasValue) query.Add($"collectionPeriod={collectionPeriod.Value}");
            if (query.Count > 0) url += $"?{string.Join("&", query)}";

            return url;
        }

        public string AddShortCourseLearnerData(long ukprn)
            => _useLegacyUrlFormats
                ? $"/learnerdata/providers/{ukprn}/shortCourses"
                : "TODO_NEW_URL_ADD_SHORT_COURSE_LEARNER_DATA_NO_ACADEMIC_YEAR";

        public string GetLearners(long ukprn, int academicYear)
            => _useLegacyUrlFormats
                ? $"/learnerdata/Learners/providers/{ukprn}/academicyears/{academicYear}/learners"
                : "TODO_NEW_URL_GET_LEARNERS";

        public string GetProviderRefData(long ukprn)
            => _useLegacyUrlFormats
                ? $"/learnerdata/reference-data/providers/{ukprn}"
                : "TODO_NEW_URL_GET_PROVIDER_REF_DATA";

        public string GetShortCourseLearnerApprovedUlns(long ukprn, int academicYear)
            => _useLegacyUrlFormats
                ? $"/learnerdata/providers/{ukprn}/academicyears/{academicYear}/shortCourses"
                : "TODO_NEW_URL_GET_SHORT_COURSE_LEARNER_APPROVED_ULNS";

        public string UpdateShortCourseLearning(long ukprn, Guid learningKey)
            => _useLegacyUrlFormats
                ? $"/learnerdata/providers/{ukprn}/shortCourses/{learningKey}"
                : "TODO_NEW_URL_UPDATE_SHORT_COURSE_LEARNING";

        public string GetShortCourseEarningsData(long ukprn, int collectionYear, byte collectionPeriod)
            => _useLegacyUrlFormats
                ? $"/learnerdata/providers/{ukprn}/collectionPeriods/{collectionYear}/{collectionPeriod}/shortCourses"
                : "TODO_NEW_URL_GET_SHORT_COURSE_EARNINGS_DATA";

        public string UpdateLearning(long ukprn, Guid learningKey)
            => _useLegacyUrlFormats
                ? $"/learnerdata/providers/{ukprn}/learning/{learningKey}"
                : "TODO_NEW_URL_UPDATE_LEARNING";

        public string GetFm36Block(long ukprn, int collectionYear, byte collectionPeriod, int? pageSize = null, int? pageNumber = null)
        {
            if (!_useLegacyUrlFormats)
            {
                return "TODO_NEW_URL_GET_FM36_BLOCK";
            }

            var url = $"/learnerdata/Learners/providers/{ukprn}/collectionPeriod/{collectionYear}/{collectionPeriod}/fm36data";

            if (pageSize.HasValue && pageNumber.HasValue)
            {
                url += $"?page={pageNumber.Value}&pageSize={pageSize.Value}";
            }

            return url;
        }

        public string DeleteLearner(long ukprn, Guid learningKey)
            => _useLegacyUrlFormats
                ? $"/learnerdata/providers/{ukprn}/learning/{learningKey}"
                : "TODO_NEW_URL_DELETE_LEARNER";

        public string DeleteShortCourse(long ukprn, Guid learningKey)
            => _useLegacyUrlFormats
                ? $"/learnerdata/providers/{ukprn}/shortCourses/{learningKey}"
                : "TODO_NEW_URL_DELETE_SHORT_COURSE";

        public string CallHealthCheck()
            => _useLegacyUrlFormats
                ? "/learnerdata/health"
                : "TODO_NEW_URL_HEALTH_CHECK";
    }
}
