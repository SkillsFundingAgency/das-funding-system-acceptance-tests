using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class ReleasePaymentsOnlyWhenProviderPaymentsAreUnfrozenStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly PaymentsFrozenEventHelper _paymentsFrozenEventHelper;
        private readonly PaymentsUnfrozenEventHelper _paymentsUnfrozenEventHelper;
        private PaymentsFrozenEvent _paymentsFrozenEvent;
        private PaymentsUnfrozenEvent _paymentsUnfrozenEvent;
        private TestSupport.Payments[] _paymentEntity;
        private readonly PaymentsMessageHandler _paymentsMessageHelper;
        private readonly PaymentsSqlClient _paymentsApiClient;
        private byte _currentCollectionPeriod;
        private string _currentCollectionYear;

        public ReleasePaymentsOnlyWhenProviderPaymentsAreUnfrozenStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _paymentsFrozenEventHelper = new PaymentsFrozenEventHelper(context);
            _paymentsUnfrozenEventHelper = new PaymentsUnfrozenEventHelper(context);
            _paymentsMessageHelper = new PaymentsMessageHandler(context);
            _paymentsApiClient = new PaymentsSqlClient();
        }

        [When(@"Employer has frozen provider payments")]
        public async Task EmployerHasFrozenProviderPayments()
        {
            _paymentsFrozenEvent = _paymentsFrozenEventHelper.CreatePaymentsFrozenMessage();

            await _paymentsFrozenEventHelper.PublishPaymentsFrozenEvent(_paymentsFrozenEvent);
        }

        [Then(@"Employer has unfrozen provider payments")]
        public async Task EmployerHasUnfrozenProviderPayments()
        {
            _paymentsUnfrozenEvent = _paymentsUnfrozenEventHelper.CreatePaymentsUnfrozenMessage();

            await _paymentsUnfrozenEventHelper.PublishPaymentsUnfrozenEvent(_paymentsUnfrozenEvent);
        }

        [Then(@"validate payments are not frozen in the payments entity")]
        public async Task ThenValidatePaymentsAreNotFrozenInThePaymentsEntity()
        {
            await Task.Delay(10000);
            await WaitHelper.WaitForIt(() =>
            {
                return _paymentsApiClient.GetPaymentsModel(_context).PaymentsFrozen == false;
            }, "Payments are still frozen in durable entity");
        }

        [Then(@"the scheduler triggers Unfunded Payment processing for following collection period")]
        public async Task SchedulerTriggersUnfundedPaymentProcessingForFollowingCollectionPeriod()
        {
            _currentCollectionPeriod = TableExtensions.Period[DateTime.Now.AddMonths(1).ToString("MMMM")];
            _currentCollectionYear = TableExtensions.CalculateAcademicYear("1");

            var _releasePaymentsCommand = new ReleasePaymentsCommand();
            _releasePaymentsCommand.CollectionPeriod = _currentCollectionPeriod;
            _releasePaymentsCommand.CollectionYear = short.Parse(_currentCollectionYear);

            await _paymentsMessageHelper.PublishReleasePaymentsCommand(_releasePaymentsCommand);
        }

        [Then(@"do not make an on-programme payment to the training provider for that apprentice")]
        public async Task ThenDoNotMakeAnOn_ProgrammePaymentToTheTrainingProviderForThatApprentice()
        {
            await WaitHelper.WaitForIt(() =>
            {
                var paymentModel = _paymentsApiClient.GetPaymentsModel(_context);

                return paymentModel?.PaymentsFrozen == true;

            }, "Payments frozen flag not set to true.");

            await WaitHelper.WaitForUnexpected(() =>
            {
                var paymentModel = _paymentsApiClient.GetPaymentsModel(_context);

                return paymentModel?.Payments != null && paymentModel.Payments.Any(p => p.SentForPayment);

            }, "Unexpected payments sent for payment.");
        }

        [Then(@"make any on-programme payments to the provider that were not paid whilst the payment status was Inactive")]
        public async Task MakeAnyOn_ProgrammePaymentsToTheProviderThatWereNotPaidWhilstThePaymentStatusWasInactive()
        {
            await WaitHelper.WaitForIt(() =>
            {
                var paymentModel = _paymentsApiClient.GetPaymentsModel(_context);

                return paymentModel?.PaymentsFrozen == false;

            }, "Payments frozen flag not set to false.");

            await WaitHelper.WaitForIt(() =>
            {
                var paymentModel = _paymentsApiClient.GetPaymentsModel(_context);

                var payments = paymentModel.Payments.Where(p => p.CollectionPeriod <= _currentCollectionPeriod
                && p.CollectionYear == short.Parse(_currentCollectionYear));

                return paymentModel.Payments.Where(p => p.CollectionPeriod <= _currentCollectionPeriod
                && p.CollectionYear == short.Parse(_currentCollectionYear)).All(p => p.SentForPayment);
            }, "Some or all expected payments were not sent for payment after provider payment status was unfrozen!");
        }
    }
}
