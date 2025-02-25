using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Common;

[Binding]
public class ReleasePaymentsStepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly PaymentsMessageHandler _messageHelper;
    private readonly EarningsSqlClient _earningsEntitySqlClient;

    public ReleasePaymentsStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _messageHelper = new PaymentsMessageHandler(_context);
    }


    [Given(@"payments are released for (.*)")]
    [When(@"payments are released for (.*)")]
    public async Task ReleasePayments(TokenisableDateTime searchDate)
    {
        var releasePaymentsCommand = new ReleasePaymentsCommand();

        releasePaymentsCommand.CollectionPeriod = TableExtensions.Period[searchDate.Value.ToString("MMMM")];
        releasePaymentsCommand.CollectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", searchDate.Value));

        await _messageHelper.PublishReleasePaymentsCommand(releasePaymentsCommand);
    }
}
