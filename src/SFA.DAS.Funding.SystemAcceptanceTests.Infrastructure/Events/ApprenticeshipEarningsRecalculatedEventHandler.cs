using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Events
{
    public class ApprenticeshipEarningsRecalculatedEventHandler : MultipleEndpointSafeEventHandler<ApprenticeshipEarningsRecalculatedEvent> { }
}
