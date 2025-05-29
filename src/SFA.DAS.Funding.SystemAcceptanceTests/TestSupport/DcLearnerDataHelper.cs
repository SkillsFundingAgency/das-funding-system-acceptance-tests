namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

internal static class DcLearnerDataHelper
{
    // only uln and learner reference number are used from this endpoint, so we can use any values here
    internal static Learner GetLearner(string uln)
    {
        var ulnLong = long.Parse(uln);

        return new Learner
        {
            Ukprn = Constants.UkPrn, 
            LearnRefNumber = $"Learner{DateTime.UtcNow.Ticks}",
            Uln = ulnLong,
            NiNumber = $"Ni{DateTime.UtcNow.Ticks}",
        };
    }

    public class Learner
    {


        public long Ukprn { get; set; }
        public string LearnRefNumber { get; set; }
        public long Uln { get; set; }
        public string NiNumber { get; set; }

        // The properties that have been commented out do actually exist in the API response
        // however, wire mock will not allow response bodies over a certain size
        // so we have removed them to keep the response size down, keeping only the properties
        // that are used in the tests.

        // public string FamilyName { get; set; }
        // public string GivenNames { get; set; }
        // public DateTime DateOfBirth { get; set; }
        // public List<LearningDelivery> LearningDeliveries { get; set; }
    }

    public class LearningDelivery
    {
        public int AimType { get; set; }
        public DateTime LearnStartDate { get; set; }
        public DateTime LearnPlanEndDate { get; set; }
        public int FundModel { get; set; }
        public int StdCode { get; set; }
        public string DelLocPostCode { get; set; }
        public string EpaOrgID { get; set; }
        public int CompStatus { get; set; }
        public DateTime LearnActEndDate { get; set; }
        public int WithdrawReason { get; set; }
        public int Outcome { get; set; }
        public DateTime AchDate { get; set; }
        public string OutGrade { get; set; }
        public int ProgType { get; set; }
    }
}