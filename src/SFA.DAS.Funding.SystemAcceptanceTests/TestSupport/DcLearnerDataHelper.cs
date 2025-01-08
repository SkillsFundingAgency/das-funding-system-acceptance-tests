using SFA.DAS.Apprenticeships.Types;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

internal static class DcLearnerDataHelper
{
    internal static Learner GetLearner(this ScenarioContext context)
    {
        var apprenticeshipCreatedEvent = context.Get<ApprenticeshipCreatedEvent>();
        return new Learner
        {
            Ukprn = apprenticeshipCreatedEvent.Episode.Ukprn,
            LearnRefNumber = $"Learner{DateTime.UtcNow.Ticks}",
            Uln = long.Parse(apprenticeshipCreatedEvent.Uln),
            FamilyName = apprenticeshipCreatedEvent.LastName,
            GivenNames = apprenticeshipCreatedEvent.FirstName,
            DateOfBirth = apprenticeshipCreatedEvent.DateOfBirth,
            NiNumber = $"Ni{DateTime.UtcNow.Ticks}",
            LearningDeliveries = new List<LearningDelivery>
            {
                new LearningDelivery
                {
                    AimType = 0,
                    LearnStartDate = apprenticeshipCreatedEvent.Episode.Prices.First().StartDate,//This may need more work in the future
                    LearnPlanEndDate = apprenticeshipCreatedEvent.Episode.Prices.Last().EndDate,//This may need more work in the future
                    FundModel = 0,
                    StdCode = 0,
                    DelLocPostCode = "string",
                    EpaOrgID = "string",
                    CompStatus = 0,
                    LearnActEndDate = apprenticeshipCreatedEvent.Episode.Prices.Last().EndDate,//This may need more work in the future
                    WithdrawReason = 0,
                    Outcome = 0,
                    AchDate = apprenticeshipCreatedEvent.Episode.Prices.First().StartDate,//This may need more work in the future
                    OutGrade = "string",
                    ProgType = 0
                }
            }
        };
    }

    public class Learner
    {
        public long Ukprn { get; set; }
        public string LearnRefNumber { get; set; }
        public long Uln { get; set; }
        public string FamilyName { get; set; }
        public string GivenNames { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string NiNumber { get; set; }
        public List<LearningDelivery> LearningDeliveries { get; set; }
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