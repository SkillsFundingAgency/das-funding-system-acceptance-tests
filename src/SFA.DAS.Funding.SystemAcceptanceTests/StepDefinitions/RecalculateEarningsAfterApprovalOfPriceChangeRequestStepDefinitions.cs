using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

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
        public async Task GivenEarningsHaveBeenCalculatedForAnApprenticeshipWithAnd(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
        {
            _calculateEarningsStepDefinitions.ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(startDate, plannedEndDate, 22500, "614");

            await _calculateEarningsStepDefinitions.TheApprenticeshipCommitmentIsApproved();
        }


        [Given(@"the total price is below or at the funding band maximum")]
        public void GivenTheTotalPriceIsBelowOrAtTheFundingBandMaximum()
        {
        }

        [Given(@"a price change request was sent on (.*)")]
        public void GivenAPriceChangeRequestWasSentOn(DateTime effectiveFromDate)
        {
            _priceChangeEffectiveFrom = effectiveFromDate;
        }

        [Given(@"the price change request has an approval date of (.*) with a new total (.*)")]
        public void GivenThePriceChangeRequestHasAnApprovalDateOfWithANewTotal(DateTime approvedDate, decimal newTotalPrice)
        {
            _priceChangeApprovedDate = approvedDate;
            _newTrainingPrice = newTotalPrice * 0.8m;
            _newAssessmentPrice = newTotalPrice * 0.2m;
        }

        [When(@"the price change is approved")]
        public async Task WhenThePriceChangeIsApproved()
        {
            _priceChangeApprovedEvent = _priceChangeMessageHandler.CreatePriceChangeApprovedMessageWithCustomValues(_newTrainingPrice, _newAssessmentPrice, _priceChangeEffectiveFrom, _priceChangeApprovedDate);

            await _priceChangeMessageHandler.PublishPriceChangeApprovedEvent(_priceChangeApprovedEvent);
        }

        [Then(@"the earnings are recalculated based on the new instalment amount of (.*) from (.*) and (.*)")]
        public async Task ThenTheEarningsAreRecalculatedBasedOnTheNewInstalmentAmountOfFromAnd(decimal newInstalmentAmount, int deliveryPeriod, int academicYear)
        {
            await _priceChangeMessageHandler.ReceiveEarningsRecalculatedEvent(_priceChangeApprovedEvent.ApprenticeshipKey);

            ApprenticeshipEarningsRecalculatedEvent recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

            recalculatedEarningsEvent.DeliveryPeriods.Where(Dp => Dp.AcademicYear >=  academicYear && Dp.Period >= deliveryPeriod).All(p => p.LearningAmount.Should().Equals(newInstalmentAmount));
        }


        [Then(@"earnings prior to (.*) and (.*) are frozen with (.*)")]
        public void ThenEarningsPriorToAndAreFrozenWith(int delivery_period, int academicYear, double oldInstalmentAmount)
        {
            var earningsApiClient = new EarningsEntityApiClient(_context);

            _earningsApprenticeshipEntity = earningsApiClient.GetEarningsEntityModel();

            var new_Earnings_Profile = _earningsApprenticeshipEntity.Model.EarningsProfile.Instalments;

            for (int i = 0; i < delivery_period-1; i++)
            {
                if (new_Earnings_Profile[i].AcademicYear <= academicYear)
                {
                    Assert.AreEqual(oldInstalmentAmount, new_Earnings_Profile[i].Amount, $"Earning prior to DeliveryPeriod {delivery_period} " +
                        $" are not frozen. Expected Amount for Delivery Period: {new_Earnings_Profile[i].DeliveryPeriod} and AcademicYear: " +
                        $" {new_Earnings_Profile[i].AcademicYear} to be {oldInstalmentAmount} but was {new_Earnings_Profile[i].Amount}");
                }
            }
        }   

        [Then(@"the history of old and new earnings is maintained with (.*) from instalment period (.*)")]
        public void ThenTheHistoryOfOldAndNewEarningsIsMaintainedWithFromInstalmentPeriod(double old_instalment_amount, int delivery_period)
        {
            var historical_instalments = _earningsApprenticeshipEntity.Model.EarningsProfileHistory[0].Record.Instalments;

            foreach (var instalment in historical_instalments)
            {
                Assert.AreEqual(old_instalment_amount, instalment.Amount, $"Expected historical earnings for period {instalment.DeliveryPeriod} to be {old_instalment_amount}, but was {instalment.Amount}");
            }
        }
    }
}