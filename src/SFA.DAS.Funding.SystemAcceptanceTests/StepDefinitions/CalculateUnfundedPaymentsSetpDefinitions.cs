using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CalculateUnfundedPaymentsSetpDefinitions
    {
        private readonly ScenarioContext _context;
        private List<Payment> _payments;
        private readonly PaymentsMessageHandler _paymentsMessageHelper;


        public CalculateUnfundedPaymentsSetpDefinitions(ScenarioContext context)
        {
            _context = context;
            _paymentsMessageHelper = new PaymentsMessageHandler(context);
        }

        [When(@"the Unfunded Payments for the remainder of the apprenticeship are determined")]
        public async Task WhenTheUnfundedPaymentsForTheRemainderOfTheApprenticeshipAreDetermined()
        {
            await _paymentsMessageHelper.ReceivePaymentsEvent(_context.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey);

            _payments = _context.Get<PaymentsGeneratedEvent>().Payments;
        }

        [Then(@"the Unfunded Payments for every earning is created in the following month")]
        public void UnfundedPaymentsForEveryEarningIsCreatedInTheFollowingMonth(Table table) => _payments.ShouldHaveCorrectPaymentsGenerated(table.ToExpectedPayments());

        [Then(@"Unfunded Payments for the appreticeship including rollup payments are calculated as below")]
        public void ThenUnfundedPaymentsForTheAppreticeshipIncludingRollupPaymentsAreCalculatedAsBelow(Table table) => _payments.ShouldHaveCorrectPaymentsGenerated(table.ToExpectedRollupPayments());
    }
}
