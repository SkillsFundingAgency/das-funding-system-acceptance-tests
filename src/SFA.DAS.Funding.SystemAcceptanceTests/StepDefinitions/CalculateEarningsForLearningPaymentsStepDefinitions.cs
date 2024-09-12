using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;
using ApprenticeshipsMessages = SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CalculateEarningsForLearningPaymentsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly ApprenticeshipMessageHandler _messageHelper;
        private readonly EarningsEntitySqlClient _earningsEntitySqlClient;
        
        private CommitmentsMessages.ApprenticeshipCreatedEvent _commitmentsApprenticeshipCreatedEvent;
        private ApprenticeshipsMessages.ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
        private EarningsEntityModel? _earningsEntity;

        private EarningsGeneratedEvent _earnings;
        private List<DeliveryPeriod> _deliveryPeriods;

        public CalculateEarningsForLearningPaymentsStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _messageHelper = new ApprenticeshipMessageHandler(_context);
            _earningsEntitySqlClient = new EarningsEntitySqlClient();
        }

        [Given(@"an apprenticeship has a start date of (.*), a planned end date of (.*), an agreed price of (.*), and a training code (.*)")]
        public void ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
        {
            _commitmentsApprenticeshipCreatedEvent = _messageHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice, trainingCode);
            _context.Set(_commitmentsApprenticeshipCreatedEvent);
        }

        [Given(@"an apprenticeship has a start date in the current month with a duration of (.*) months")]
        public void GivenAnApprenticeshipHasAStartDateInTheCurrentMonthWithADurationOfMonths(int duration)
        {
            var currentDate = DateTime.Today;
            var startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
            
            var futureDate = currentDate.AddMonths(duration-1);
            var plannedEndDate = new DateTime(futureDate.Year, futureDate.Month, DateTime.DaysInMonth(futureDate.Year, futureDate.Month));

            ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(startDate, plannedEndDate, 30000, "614");
        }


        [Given(@"an apprenticeship with start date over (.*) months ago and duration of (.*) months and an agreed price of (.*), and a training code (.*)")]
        public void ApprenticeshipWithStartDateOverMonthsAgoAndDurationOfMonthsAndAnAgreedPriceOfAndATrainingCode(int monthsSinceStart, int duration, decimal agreedPrice, string trainingCode)
        {
            DateTime today = DateTime.Today;
            var startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-monthsSinceStart);
            var plannedEndDate = startDate.AddMonths(duration).AddDays(-1);

            _commitmentsApprenticeshipCreatedEvent = _messageHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice, trainingCode);
            _context.Set(_commitmentsApprenticeshipCreatedEvent);
        }


        [Given(@"the apprenticeship learner has a date of birth (.*)")]
        public void AddDateOfBirthToCommitmentsApprenticeshipCreatedEvent(DateTime dob)
        {
            _commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
            
            _messageHelper.UpdateApprenticeshipCreatedMessageWithDoB(_commitmentsApprenticeshipCreatedEvent, dob);

        }
        
        [When(@"the agreed price is (below|above) the funding band maximum for the selected course")]
        public void VerifyFundingBandMaxValue(string condition)
        {
            if (condition == "below") Assert.Less(_commitmentsApprenticeshipCreatedEvent.PriceEpisodes.MaxBy(x => x.FromDate).Cost, _apprenticeshipCreatedEvent.Episode.Prices.MaxBy(x => x.StartDate).FundingBandMaximum);
            else Assert.Greater(_commitmentsApprenticeshipCreatedEvent.PriceEpisodes.MaxBy(x => x.FromDate).Cost, _apprenticeshipCreatedEvent.Episode.Prices.MaxBy(x => x.StartDate).FundingBandMaximum);
        }

        [Given(@"the apprenticeship commitment is approved")]
        [When(@"the apprenticeship commitment is approved")]
        public async Task TheApprenticeshipCommitmentIsApproved()
        {
            _commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
            await _messageHelper.PublishApprenticeshipApprovedMessage(_commitmentsApprenticeshipCreatedEvent);

            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipsMessages.ApprenticeshipCreatedEvent>();

            _earnings = _context.Get<EarningsGeneratedEvent>();
            _deliveryPeriods = _earnings.DeliveryPeriods;

            _context.Set(_deliveryPeriods);

            await WaitHelper.WaitForIt(() =>
            {
                _earningsEntity = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

                if (_earningsEntity != null)
                {
                    return true;
                }
                return false;
            }, "Failed to find Earnings Entity");

            _context.Set(_earningsEntity.Model.ApprenticeshipEpisodes.MaxBy(x => x.Prices.MaxBy(y => y.ActualStartDate).ActualStartDate).EarningsProfile.EarningsProfileId, "InitialEarningsProfileId");
        }


        [Then(@"80% of the agreed price is calculated as total on-program payment which is divided equally into number of planned months (.*)")]
        [Then(@"Agreed price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
        [Then(@"Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
        public void VerifyInstalmentAmountIsCalculatedEquallyIntoAllEarningMonths(decimal instalmentAmount)
        {
            _deliveryPeriods.ForEach(dp => dp.LearningAmount.Should().Be(instalmentAmount));
        }

        [Then(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
        public void VerifyThePlannedDurationMonthsWithinTheEarningsGenerated(short numberOfInstalments)
        {
            _deliveryPeriods.Should().HaveCount(numberOfInstalments);
        }

        [Given(@"the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year")]
        [Then(@"the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year")]
        public void ThenTheDeliveryPeriodForEachInstalmentMustBeTheDeliveryPeriodFromTheCollectionCalendarWithAMatchingCalendarMonthYear(Table table)
        {
            _deliveryPeriods.ShouldHaveCorrectFundingPeriods(table.ToExpectedPeriods());
        }

        [Then(@"the total completion amount (.*) should be calculated as 20% of the adjusted price")]
        public void VerifyCompletionAmountIsCalculatedCorrectly(decimal completionAmount)
        {
            var apprenticeshipEntity = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

            Assert.AreEqual(completionAmount, apprenticeshipEntity?.Model.ApprenticeshipEpisodes.MaxBy(x => x.Prices.MaxBy(y => y.ActualStartDate).ActualStartDate).EarningsProfile.CompletionPayment);
        }

        [Then(@"the leaners age (.*) at the start of the course and funding line type (.*) must be calculated")]
        public void ValidateAgeAndFundingLineTypeCalculated(int age, string fundingLineType)
        {
            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipsMessages.ApprenticeshipCreatedEvent>();
            Assert.AreEqual(_apprenticeshipCreatedEvent.Episode.AgeAtStartOfApprenticeship, age, $"Expected age is: {age} but found age: {_apprenticeshipCreatedEvent.Episode.AgeAtStartOfApprenticeship}");
            
            _deliveryPeriods.ShouldHaveCorrectFundingLineType(fundingLineType);
        }
    }
}
