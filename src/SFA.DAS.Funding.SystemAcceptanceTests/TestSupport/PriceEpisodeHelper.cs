using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    public class PriceEpisodeHelper
    {
        public PriceEpisode[] CreateSinglePriceEpisodeUsingStartDate(DateTime fromDate, decimal cost)
        {
            var episode = new PriceEpisode
            {
                FromDate = fromDate,
                TrainingPrice = cost * 0.8m,
                EndPointAssessmentPrice = cost * 0.2m,
                Cost = cost
            };
            return [episode];
        }
    }
}
