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
    private readonly PaymentsSqlClient _paymentsApiClient;
    private readonly PaymentsFunctionsClient _paymentsFunctionsClient;

    public ReleasePaymentsOnlyWhenProviderPaymentsAreUnfrozenStepDefinitions(ScenarioContext context, PaymentsSqlClient paymentsSqlClient, PaymentsFunctionsClient paymentsFunctionsClient)
    {
        _context = context;
        _paymentsApiClient = paymentsSqlClient;
        _paymentsFunctionsClient = paymentsFunctionsClient;
    }

    [When(@"Employer has frozen provider payments")]
    public async Task EmployerHasFrozenProviderPayments()
    {
        var paymentsFrozenEvent = _context.CreatePaymentsFrozenMessage();
        await PaymentsFrozenEventHelper.PublishPaymentsFrozenEvent(paymentsFrozenEvent);
    }

    [Then(@"Employer has unfrozen provider payments")]
    public async Task EmployerHasUnfrozenProviderPayments()
    {
        var paymentsUnfrozenEvent = _context.CreatePaymentsUnfrozenMessage();
        await PaymentsUnfrozenEventHelper.PublishPaymentsUnfrozenEvent(paymentsUnfrozenEvent);
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

        var testData = _context.Get<TestData>();
        testData.CurrentCollectionYear = collectionYear;
        testData.CurrentCollectionPeriod = collectionPeriod;

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
        var testData = _context.Get<TestData>();

        await WaitHelper.WaitForIt(() =>
        {
            var paymentModel = _paymentsApiClient.GetPaymentsModel(_context);

            return paymentModel?.PaymentsFrozen == false;

        }, "Payments frozen flag not set to false.");

        await WaitHelper.WaitForIt(() =>
        {
            var paymentModel = _paymentsApiClient.GetPaymentsModel(_context);

            var payments = paymentModel.Payments.Where(p => p.CollectionPeriod <= testData.CurrentCollectionPeriod
            && p.CollectionYear == short.Parse(testData.CurrentCollectionYear));

            return paymentModel.Payments.Where(p => p.CollectionPeriod <= testData.CurrentCollectionPeriod
            && p.CollectionYear == short.Parse(testData.CurrentCollectionYear)).All(p => p.SentForPayment);
        }, "Some or all expected payments were not sent for payment after provider payment status was unfrozen!");
    }
}
