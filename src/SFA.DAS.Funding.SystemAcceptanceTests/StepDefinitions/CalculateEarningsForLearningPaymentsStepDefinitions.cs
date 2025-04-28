using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using ApprenticeshipsMessages = SFA.DAS.Apprenticeships.Types;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class CalculateEarningsForLearningPaymentsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly ApprenticeshipMessageHandler _messageHelper;
        private readonly EarningsSqlClient _earningsEntitySqlClient;
        
        private CommitmentsMessages.ApprenticeshipCreatedEvent _commitmentsApprenticeshipCreatedEvent;
        private ApprenticeshipsMessages.ApprenticeshipCreatedEvent _apprenticeshipCreatedEvent;

        public CalculateEarningsForLearningPaymentsStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _messageHelper = new ApprenticeshipMessageHandler(_context);
            _earningsEntitySqlClient = new EarningsSqlClient();
        }


        [When(@"the agreed price is (below|above) the funding band maximum for the selected course")]
        public void VerifyFundingBandMaxValue(string condition)
        {
            _commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipsMessages.ApprenticeshipCreatedEvent>();

            if (condition == "below") Assert.Less(_commitmentsApprenticeshipCreatedEvent.PriceEpisodes.MaxBy(x => x.FromDate).Cost, _apprenticeshipCreatedEvent.Episode.Prices.MaxBy(x => x.StartDate).FundingBandMaximum);
            else Assert.Greater(_commitmentsApprenticeshipCreatedEvent.PriceEpisodes.MaxBy(x => x.FromDate).Cost, _apprenticeshipCreatedEvent.Episode.Prices.MaxBy(x => x.StartDate).FundingBandMaximum);
        }

        [Then(@"80% of the agreed price is calculated as total on-program payment which is divided equally into number of planned months (.*)")]
        [Then(@"Agreed price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
        [Then(@"Funding band maximum price is used to calculate the on-program earnings which is divided equally into number of planned months (.*)")]
        public void VerifyInstalmentAmountIsCalculatedEquallyIntoAllEarningMonths(decimal instalmentAmount)
        {
            var deliveryPeriods = _context.Get<List<DeliveryPeriod>>();
            deliveryPeriods.ForEach(dp => dp.LearningAmount.Should().Be(instalmentAmount));
        }

        [Given(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
        [Then(@"the planned number of months must be the number of months from the start date to the planned end date (.*)")]
        public void VerifyThePlannedDurationMonthsWithinTheEarningsGenerated(short numberOfInstalments)
        {
            var deliveryPeriods = _context.Get<List<DeliveryPeriod>>();
            deliveryPeriods.Should().HaveCount(numberOfInstalments);
        }

        [Given(@"the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year")]
        [Then(@"the delivery period for each instalment must be the delivery period from the collection calendar with a matching calendar month/year")]
        public void ThenTheDeliveryPeriodForEachInstalmentMustBeTheDeliveryPeriodFromTheCollectionCalendarWithAMatchingCalendarMonthYear(Table table)
        {
            var deliveryPeriods = _context.Get<List<DeliveryPeriod>>();
            deliveryPeriods.ShouldHaveCorrectFundingPeriods(table.ToExpectedPeriods());
        }

        [Then(@"the total completion amount (.*) should be calculated as 20% of the adjusted price")]
        public void VerifyCompletionAmountIsCalculatedCorrectly(decimal completionAmount)
        {
            var apprenticeshipEntity = _earningsEntitySqlClient.GetEarningsEntityModel(_context);

            Assert.AreEqual(completionAmount, apprenticeshipEntity?.Episodes.MaxBy(x => x.Prices.MaxBy(y => y.StartDate).StartDate).EarningsProfile.CompletionPayment);
        }

        [Then(@"the leaners age (.*) at the start of the course and funding line type (.*) must be calculated")]
        public void ValidateAgeAndFundingLineTypeCalculated(int age, string fundingLineType)
        {
            _apprenticeshipCreatedEvent = _context.Get<ApprenticeshipsMessages.ApprenticeshipCreatedEvent>();
            Assert.AreEqual(_apprenticeshipCreatedEvent.Episode.AgeAtStartOfApprenticeship, age, $"Expected age is: {age} but found age: {_apprenticeshipCreatedEvent.Episode.AgeAtStartOfApprenticeship}");
            
            var deliveryPeriods = _context.Get<List<DeliveryPeriod>>();
            deliveryPeriods.ShouldHaveCorrectFundingLineType(fundingLineType);
        //}

        //[Then(@"the (first|second) incentive earning (is|is not) generated for provider & employer")]
        //public void VerifyIncentiveEarnings(string incentiveEarningNumber, string outcome)
        //{
        //    _commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        //    var additionalPayments = _context
        //        .Get<EarningsApprenticeshipModel>(ContextKeys.InitialEarningsApprenticeshipModel)
        //        .Episodes
        //        .SingleOrDefault()
        //        ?.AdditionalPayments;

        //    additionalPayments.Should().NotBeNull("No episode found on earnings apprenticeship model");

        //    var incentiveExpected = outcome == "is";
        //    var expectation = incentiveExpected ? "Expected" : "Not Expected";

        //    switch (incentiveEarningNumber)
        //    {
        //        case "first":
        //            additionalPayments!
        //                .IncentivePaymentExists(_commitmentsApprenticeshipCreatedEvent.StartDate, 1, AdditionalPaymentType.ProviderIncentive)
        //                .Should().Be(incentiveExpected,$"First Incentive Earning {expectation} For Provider");
        //            additionalPayments!
        //                .IncentivePaymentExists(_commitmentsApprenticeshipCreatedEvent.StartDate, 1, AdditionalPaymentType.EmployerIncentive)
        //                .Should().BeTrue($"First Incentive Earning {expectation} For Employer");
        //            break;
        //        case "second":
        //            additionalPayments!
        //                .IncentivePaymentExists(_commitmentsApprenticeshipCreatedEvent.StartDate, 2, AdditionalPaymentType.ProviderIncentive)
        //                .Should().BeTrue($"Second Incentive Earning {expectation} For Provider");
        //            additionalPayments!
        //                .IncentivePaymentExists(_commitmentsApprenticeshipCreatedEvent.StartDate, 2, AdditionalPaymentType.EmployerIncentive)
        //                .Should().BeTrue($"Second Incentive Earning {expectation} For Employer");
        //            break;
        //        default:
        //            throw new Exception("Step definition requires 'first' or 'second' to be specified for incentive earning");
        //    }
        }
    }
}
