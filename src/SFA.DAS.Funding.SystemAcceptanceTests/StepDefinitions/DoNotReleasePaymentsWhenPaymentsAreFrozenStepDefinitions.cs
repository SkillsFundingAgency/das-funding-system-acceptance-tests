using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using TechTalk.SpecFlow;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class DoNotReleasePaymentsWhenPaymentsAreFrozenStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly PaymentsFrozenEventHelper _paymentsFrozenEventHelper;
        private PaymentsFrozenEvent _paymentsFrozenEvent;
        private TestSupport.Payments[] _paymentEntity;

        public DoNotReleasePaymentsWhenPaymentsAreFrozenStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _paymentsFrozenEventHelper = new PaymentsFrozenEventHelper(context);
        }

        [When(@"Employer has frozen provider payments")]
        public async Task WhenEmployerHasFrozenProviderPayments()
        {
            _paymentsFrozenEvent = _paymentsFrozenEventHelper.CreatePaymentsFrozenMessage();

            await _paymentsFrozenEventHelper.PublishPaymentsFrozenEvent(_paymentsFrozenEvent);
        }

        [Then(@"do not make an on-programme payment to the training provider for that apprentice")]
        public async Task ThenDoNotMakeAnOn_ProgrammePaymentToTheTrainingProviderForThatApprentice()
        {
            var apiClient = new PaymentsEntityApiClient(_context);

            bool paymentsStatus;

            await WaitHelper.WaitForUnexpected(() =>
            {
                _paymentEntity = apiClient.GetPaymentsEntityModel().Model.Payments;

                paymentsStatus = apiClient.GetPaymentsEntityModel().Model.PaymentsFrozen;

                return (_paymentEntity.All(p => p.SentForPayment) && !paymentsStatus);

            }, "PaymentsFrozen flag is false and/or unexpected payments were released!");
        }
    }
}
