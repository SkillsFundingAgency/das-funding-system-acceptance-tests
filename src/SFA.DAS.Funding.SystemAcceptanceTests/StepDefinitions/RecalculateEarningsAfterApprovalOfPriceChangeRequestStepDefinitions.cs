using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Payments.Messages.Core.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class RecalculateEarningsAfterApprovalOfPriceChangeRequestStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly CalculateEarningsForLearningPaymentsStepDefinitions _calculateEarningsStepDefinitions;
        private PriceChangeMessageHandler _priceChangeMessageHandler;
        private PriceChangeApprovedEvent _priceChangeApprovedEvent;
        private EarningsEntityModel _earningsApprenticeshipEntity;
        private DateTime _priceChangeEffectiveFrom;
        private DateTime _priceChangeApprovedDate;
        private decimal _newTrainingPrice;
        private decimal _newAssessmentPrice;

        public RecalculateEarningsAfterApprovalOfPriceChangeRequestStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _calculateEarningsStepDefinitions = new CalculateEarningsForLearningPaymentsStepDefinitions(_context);
            _priceChangeMessageHandler = new PriceChangeMessageHandler(_context);
        }

        [Given(@"earnings have been calculated for an apprenticeship with (.*), (.*), (.*), and (.*)")]
        public async Task EarningsHaveBeenCalculatedForAnApprenticeshipWithAnd(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
        {
            _calculateEarningsStepDefinitions.ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(startDate, plannedEndDate, agreedPrice, trainingCode);

            await _calculateEarningsStepDefinitions.TheApprenticeshipCommitmentIsApproved();
        }


        [Given(@"the total price is above or below or at the funding band maximum")]
        public void TotalPriceIsBelowOrAtTheFundingBandMaximum()
        {
        }

        [Given(@"a price change request was sent on (.*)")]
        public void PriceChangeRequestWasSentOn(DateTime effectiveFromDate)
        {
            _priceChangeEffectiveFrom = effectiveFromDate;
        }

        [Given(@"the price change request has an approval date of (.*) with a new total (.*)")]
        public void PriceChangeRequestHasAnApprovalDateOfWithANewTotal(DateTime approvedDate, decimal newTotalPrice)
        {
            _priceChangeApprovedDate = approvedDate;
            _newTrainingPrice = newTotalPrice * 0.8m;
            _newAssessmentPrice = newTotalPrice * 0.2m;
        }

        [When(@"the price change is approved")]
        public async Task PriceChangeIsApproved()
        {
            _priceChangeApprovedEvent = _priceChangeMessageHandler.CreatePriceChangeApprovedMessageWithCustomValues(_newTrainingPrice, _newAssessmentPrice, _priceChangeEffectiveFrom, _priceChangeApprovedDate);

            await _priceChangeMessageHandler.PublishPriceChangeApprovedEvent(_priceChangeApprovedEvent);
        }

        [Then(@"the earnings are recalculated based on the new instalment amount of (.*) from (.*) and (.*)")]
        public async Task EarningsAreRecalculatedBasedOnTheNewInstalmentAmountOfFromAnd(decimal newInstalmentAmount, int deliveryPeriod, int academicYear)
        {
            await _priceChangeMessageHandler.ReceiveEarningsRecalculatedEvent(_priceChangeApprovedEvent.ApprenticeshipKey);

            ApprenticeshipEarningsRecalculatedEvent recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

            recalculatedEarningsEvent.DeliveryPeriods.Where(Dp => Dp.AcademicYear >= academicYear && Dp.Period >= deliveryPeriod).All(p => p.LearningAmount.Should().Equals(newInstalmentAmount));
        }


        [Then(@"earnings prior to (.*) and (.*) are frozen with (.*)")]
        public void EarningsPriorToAndAreFrozenWith(int delivery_period, int academicYear, double oldInstalmentAmount)
        {
            var earningsApiClient = new EarningsEntityApiClient(_context);

            _earningsApprenticeshipEntity = earningsApiClient.GetEarningsEntityModel();

            var newEarningsProfile = _earningsApprenticeshipEntity.Model.EarningsProfile.Instalments;

            for (int i = 0; i < delivery_period - 1; i++)
            {
                if (newEarningsProfile[i].AcademicYear <= academicYear)
                {
                    Assert.AreEqual(oldInstalmentAmount, newEarningsProfile[i].Amount, $"Earning prior to DeliveryPeriod {delivery_period} " +
                        $" are not frozen. Expected Amount for Delivery Period: {newEarningsProfile[i].DeliveryPeriod} and AcademicYear: " +
                        $" {newEarningsProfile[i].AcademicYear} to be {oldInstalmentAmount} but was {newEarningsProfile[i].Amount}");
                }
            }
        }

        [Then(@"the history of old earnings is maintained with (.*)")]
        public async Task HistoryOfOldEarningsIsMaintained(double old_instalment_amount)
        {
            await WaitHelper.WaitForIt(() =>
            {
                var historicalInstalments = _earningsApprenticeshipEntity.Model.EarningsProfileHistory[0].Record.Instalments;

                if (historicalInstalments != null)
                {
                    foreach (var instalment in historicalInstalments)
                    {
                        Assert.AreEqual(old_instalment_amount, instalment.Amount, $"Expected historical earnings amount to be {old_instalment_amount}, but was {instalment.Amount}");
                    }
                    return true;
                }
                return false;
            }, "Failed to find installments in Earnings Profile History");
        }
    }
}