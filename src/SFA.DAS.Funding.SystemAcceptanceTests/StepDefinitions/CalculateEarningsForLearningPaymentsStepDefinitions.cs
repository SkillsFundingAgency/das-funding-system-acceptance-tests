using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Models;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CalculateEarningsForLearningPaymentsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly ApprenticeshipMessageHandler _messageHelper;
        private ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;
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
            _apprenticeshipCreatedEvent = _messageHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice, trainingCode);
            _context.Set(_apprenticeshipCreatedEvent);
        }


        [When(@"the agreed price is (below|above) the funding band maximum (.*) for the selected course")]
        public void VerifyFundingBandMaxValue(string condition, decimal fundingBandMax)
        {
            //Assert.AreEqual(_earnings.FundingPeriods, fundingBandMax, "Unexpected FundingBandMax value found");

            if (condition == "below")
            {
                Assert.Less(_apprenticeshipCreatedEvent.PriceEpisodes[0].Cost, _earnings.ApprenticeshipKey);
            }
            else
            {
                Assert.Greater(_apprenticeshipCreatedEvent.PriceEpisodes[0].Cost, _earnings.ApprenticeshipKey);
            }
        }

        [When(@"the apprenticeship commitment is approved")]
        public async Task TheApprenticeshipCommitmentIsApproved()
        {
            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipCreatedEvent>();
            await _messageHelper.PublishApprenticeshipApprovedMessage(_apprenticeshipCreatedEvent);
            _earnings = _messageHelper.ReadEarningsGeneratedMessage(_apprenticeshipCreatedEvent);
            _fundingPeriod = _earnings.FundingPeriods.First();

            _context.Set(_earnings);
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
    }
}
