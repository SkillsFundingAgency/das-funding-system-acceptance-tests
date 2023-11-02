using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using ApprenticeshipsMessages = SFA.DAS.Apprenticeships.Types;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Funding.ApprenticeshipEarnings.Types;
using FluentAssertions.Extensions;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public  class RecalculateEarningsAfterApprovalOfPriceChangeRequestStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly CalculateEarningsForLearningPaymentsStepDefinitions _calculateEarningsStepDefinitions;
        private PriceChangeMessageHandler _priceChangeMessageHandler;
        private PriceChangeApprovedEvent _priceChangeApprovedEvent;
        private CommitmentsMessages.ApprenticeshipCreatedEvent _commitmentsApprenticeshipCreatedEvent;
        private ApprenticeshipsMessages.ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
        private EarningsGeneratedEvent _earnings;
        private List<DeliveryPeriod> _deliveryPeriods;

        public RecalculateEarningsAfterApprovalOfPriceChangeRequestStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _calculateEarningsStepDefinitions = new CalculateEarningsForLearningPaymentsStepDefinitions(_context);
            _priceChangeMessageHandler = new PriceChangeMessageHandler(_context);
        }

        [Given(@"earnings have been calculated for an apprenticeship in the pilot")]
        public async Task GivenEarningsHaveBeenCalculatedForAnApprenticeshipInThePilot()
        {
            DateTime startDate = new DateTime(2022, 8, 15);
            DateTime plannedEndDate = new DateTime(2023, 8, 15);
            _calculateEarningsStepDefinitions.ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(startDate, plannedEndDate, 22500, "614");

            await _calculateEarningsStepDefinitions.TheApprenticeshipCommitmentIsApproved();
        }

        [Given(@"the total price is below or at the funding band maximum")]
        public void GivenTheTotalPriceIsBelowOrAtTheFundingBandMaximum()
        {
        }

        [Given(@"a price change request was sent before the end of R14 of the current academic year")]
        public void GivenAPriceChangeRequestWasSentBeforeTheEndOfROfTheCurrentAcademicYearYearX()
        {
            DateTime effectiveFrom = new DateTime(2023, 1, 1);
            DateTime approvedDate = new DateTime(2023, 10, 15);
            _priceChangeApprovedEvent = _priceChangeMessageHandler.CreatePriceChangeApprovedMessageWithCustomValues(25000, 2000, effectiveFrom, approvedDate);
        }

        [Given(@"the price change request is for a new total price up to or at the funding band maximum")]
        public void GivenThePriceChangeRequestIsForANewTotalPriceUpToOrAtTheFundingBandMaximum()
        {
        }

        [When(@"the change is approved by the other party before the end of year X")]
        public async Task WhenTheChangeIsApprovedByTheOtherPartyBeforeTheEndOfYearX()
        {
            await _priceChangeMessageHandler.PublishPriceChangeApprovedEvent(_priceChangeApprovedEvent);
        }

        [Then(@"the earnings are recalculated based on the new price")]
        public async Task ThenTheEarningsAreRecalculatedBasedOnTheNewPrice()
        {
            await _priceChangeMessageHandler.ReceiveEarningsRecalculatedEvent(_priceChangeApprovedEvent.ApprenticeshipKey);

            ApprenticeshipEarningsRecalculatedEvent recalculatedEarningsEvent = _context.Get<ApprenticeshipEarningsRecalculatedEvent>();

            Assert.AreEqual(1800, recalculatedEarningsEvent.DeliveryPeriods[11].LearningAmount);

        }

        [Then(@"the history of old and new earnings is maintained")]
        public void ThenTheHistoryOfOldAndNewEarningsIsMaintained()
        {
        }


    }
}
