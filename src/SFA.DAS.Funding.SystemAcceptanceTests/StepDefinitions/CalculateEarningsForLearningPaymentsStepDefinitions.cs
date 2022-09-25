using Newtonsoft.Json;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;
using ApprenticeshipsMessages = SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CalculateEarningsForLearningPaymentsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly ApprenticeshipMessageHandler _messageHelper;
        private CommitmentsMessages.ApprenticeshipCreatedEvent _commitmentsApprenticeshipCreatedEvent;
        private ApprenticeshipsMessages.ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
        private EarningsGeneratedEvent _earnings;
        private FundingPeriod _fundingPeriod;

        public CalculateEarningsForLearningPaymentsStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _messageHelper = new ApprenticeshipMessageHandler(_context);
        }

        [Given(@"an apprenticeship has a start date of (.*), a planned end date of (.*), an agreed price of (.*), and a training code (.*)")]
        public void GivenAnApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(DateTime startDate, DateTime plannedEndDate, decimal agreedPrice, string trainingCode)
        {
            _commitmentsApprenticeshipCreatedEvent = _messageHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice, trainingCode);
            _context.Set(_commitmentsApprenticeshipCreatedEvent);
        }

        [Given(@"the apprenticeship learner has a date of birth (.*)")]
        public void AddDateOfBirthToCommitmentsApprenticeshipCreatedEvent(DateTime dob) => _commitmentsApprenticeshipCreatedEvent.DateOfBirth = dob;

        [When(@"the agreed price is (below|above) the funding band maximum for the selected course")]
        public void VerifyFundingBandMaxValue(string condition)
        {
            if (condition == "below") Assert.Less(_commitmentsApprenticeshipCreatedEvent.PriceEpisodes[0].Cost, _apprenticeshipCreatedEvent.FundingBandMaximum);
            else Assert.Greater(_commitmentsApprenticeshipCreatedEvent.PriceEpisodes[0].Cost, _apprenticeshipCreatedEvent.FundingBandMaximum);
        }

        [When(@"the apprenticeship commitment is approved")]
        public async Task TheApprenticeshipCommitmentIsApproved()
        {
            _commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
            await _messageHelper.PublishApprenticeshipApprovedMessage(_commitmentsApprenticeshipCreatedEvent);

            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipsMessages.ApprenticeshipCreatedEvent>();
            _earnings = _context.Get<EarningsGeneratedEvent>();
            _fundingPeriod = _earnings.FundingPeriods.First();

            _context.Set(_fundingPeriod);
        }


        [Then(@"80% of the agreed price is calculated as total on-program payment which is divided equally into number of planned months (.*)")]
        [Then(@"Agreed price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
        [Then(@"Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
        public void VerifyInstalmentAmountIsCalculatedEquallyIntoAllEarningMonths(decimal instalmentAmount)
        {
            _context.Get<FundingPeriod>().DeliveryPeriods.ForEach(dp => dp.LearningAmount.Should().Be(instalmentAmount));
        }

        [Then(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
        public void VerifyThePlannedDurationMonthsWithinTheEarningsGenerated(short numberOfInstalments)
        {
            _context.Get<FundingPeriod>().DeliveryPeriods.Should().HaveCount(numberOfInstalments);
        }

        [Then(@"the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year")]
        public void ThenTheDeliveryPeriodForEachInstalmentMustBeTheDeliveryPeriodFromTheCollectionCalendarWithAMatchingCalendarMonthYear(Table table)
        {
            var deliveryPeriods = _context.Get<FundingPeriod>().DeliveryPeriods;

            deliveryPeriods.ShouldHaveCorrectFundingPeriods(table.ToExpectedPeriods());
        }

        [Then(@"the total completion amount (.*) should be calculated as 20% of the adjusted price")]
        public void VerifyCompletionAmountIsCalculatedCorrectly(decimal completionAmount)
        {
            var apiClient = new ApprenticeshipEntityApiClient(_context);

            var response = apiClient.Execute();

            var jsonString = response.Result.Content.ReadAsStringAsync();

            var apprenticeshipEntity = JsonConvert.DeserializeObject<ApprenticeshipEntityModel>(jsonString.Result);

            Assert.AreEqual(completionAmount, apprenticeshipEntity?.Model.EarningsProfile.CompletionPayment);
        }

        [Then(@"the leaners age (.*) at the start of the course and funding line type (.*) must be calculated")]
        public void ValidateAgeAndFundingLineTypeCalculated(int age, string fundingLineType)
        {
            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipsMessages.ApprenticeshipCreatedEvent>();
            Assert.AreEqual(_apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship, age, $"Expected age is: {age} but found age: {_apprenticeshipCreatedEvent.AgeAtStartOfApprenticeship}");
            
            var deliveryPeriods = _context.Get<FundingPeriod>().DeliveryPeriods;
            deliveryPeriods.ShouldHaveCorrectFundingLineType(fundingLineType);
        }
    }
}
