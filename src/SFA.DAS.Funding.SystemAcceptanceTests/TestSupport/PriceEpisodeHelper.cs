using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
