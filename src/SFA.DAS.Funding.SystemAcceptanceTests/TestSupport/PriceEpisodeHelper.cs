using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    public class PriceEpisodeHelper
    {
        public PriceEpisode[] CreateSinglePriceEpisodeUsingStartDate(DateTime fromDate, decimal cost)
        {
            PriceEpisode episode = new PriceEpisode();
            episode.FromDate = fromDate;
            episode.Cost = cost;    

            return new PriceEpisode[] { episode } ;
        }
    }
}
