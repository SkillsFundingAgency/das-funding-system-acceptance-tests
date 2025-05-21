using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ReleasePaymentsOnlyWhenProviderPaymentsAreUnfrozenStepDefinitions
{
    private readonly ScenarioContext _context;
    private PaymentsFrozenEvent _paymentsFrozenEvent;
    private PaymentsUnfrozenEvent _paymentsUnfrozenEvent;
    private readonly PaymentsSqlClient _paymentsApiClient;
    private readonly PaymentsFunctionsClient _paymentsFunctionsClient;

    public ReleasePaymentsOnlyWhenProviderPaymentsAreUnfrozenStepDefinitions(ScenarioContext context)
    {
        _context = context;
        _paymentsApiClient = new PaymentsSqlClient();
        _paymentsFunctionsClient = new PaymentsFunctionsClient();
    }

    [When(@"Employer has frozen provider payments")]
    public async Task EmployerHasFrozenProviderPayments()
    {
        _paymentsFrozenEvent = _context.CreatePaymentsFrozenMessage();
        await PaymentsFrozenEventHelper.PublishPaymentsFrozenEvent(_paymentsFrozenEvent);
    }

    [Then(@"Employer has unfrozen provider payments")]
    public async Task EmployerHasUnfrozenProviderPayments()
    {
        _paymentsUnfrozenEvent = _context.CreatePaymentsUnfrozenMessage();
        await PaymentsUnfrozenEventHelper.PublishPaymentsUnfrozenEvent(_paymentsUnfrozenEvent);
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
        var collectionPeriod = TableExtensions.Period[DateTime.Now.AddMonths(1).ToString("MMMM")];
        var collectionYear = TableExtensions.CalculateAcademicYear("1");

        _context.Set(collectionYear, ContextKeys.CurrentCollectionYear);
        _context.Set(collectionPeriod, ContextKeys.CurrentCollectionPeriod);

        await _paymentsFunctionsClient.InvokeReleasePaymentsHttpTrigger(collectionPeriod,
            Convert.ToInt16(collectionYear));
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
        var currentCollectionYear = _context.Get<string>(ContextKeys.CurrentCollectionYear);
        var currentCollectionPeriod = _context.Get<byte>(ContextKeys.CurrentCollectionPeriod);

        await WaitHelper.WaitForIt(() =>
        {
            var paymentModel = _paymentsApiClient.GetPaymentsModel(_context);

            return paymentModel?.PaymentsFrozen == false;

        }, "Payments frozen flag not set to false.");

        await WaitHelper.WaitForIt(() =>
        {
            var paymentModel = _paymentsApiClient.GetPaymentsModel(_context);

            var payments = paymentModel.Payments.Where(p => p.CollectionPeriod <= currentCollectionPeriod
            && p.CollectionYear == short.Parse(currentCollectionYear));

            return paymentModel.Payments.Where(p => p.CollectionPeriod <= currentCollectionPeriod
            && p.CollectionYear == short.Parse(currentCollectionYear)).All(p => p.SentForPayment);
        }, "Some or all expected payments were not sent for payment after provider payment status was unfrozen!");
    }
}
