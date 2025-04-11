using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class Fm36StepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly EarningsOuterClient _earningsOuterClient;
    private Helpers.Sql.Apprenticeship? _apprenticeship;
    private EarningsApprenticeshipModel? _earnings;

    public Fm36StepDefinitions(ScenarioContext context)
    {
        _context = context;
        _earningsOuterClient = new EarningsOuterClient();
    }

    [Given(@"the fm36 data is retrieved for (.*)")]
    [When(@"the fm36 data is retrieved for (.*)")]
    [Then(@"the fm36 data is retrieved for (.*)")]
    public async Task GetFm36Data(TokenisableDateTime searchDate)
    {
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
        var collectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", searchDate.Value));
        var collectionPeriod = TableExtensions.Period[searchDate.Value.ToString("MMMM")];

        var fm36 = await _earningsOuterClient.GetFm36Block(apprenticeshipCreatedEvent.ProviderId, collectionYear,
            collectionPeriod);

        _context.Set(fm36);
    }

    [Then("incentives earnings are generated for learners aged 15")]
    [Then(@"fm36 data exists for that apprenticeship")]
    public async Task Fm36DataExists()
    {
        var fm36 = _context.Get<List<FM36Learner>>();
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var earningsSqlClient = new EarningsSqlClient();

        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        _apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);
        _earnings = earningsSqlClient.GetEarningsEntityModel(_context);

        // get your learner data 

        var fm36Learner = fm36.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);

        var expectedPriceEpisodeIdentifier = "25-" + apprenticeshipCreatedEvent.TrainingCode + "-" +
                                             apprenticeshipCreatedEvent.ActualStartDate?.ToString("dd/MM/yyyy");
        var priceEpisodeInstalmentsThisPeriod = (DateTime.Today >= apprenticeshipCreatedEvent.ActualStartDate &&
                                                 DateTime.Today <= apprenticeshipCreatedEvent.EndDate)
            ? 1
            : 0;
        var totalPrice = _earnings?.Episodes.FirstOrDefault()?.Prices.FirstOrDefault()?.AgreedPrice;
        var onProgPayment = _earnings?.Episodes.FirstOrDefault()?.EarningsProfile.OnProgramTotal;
        var completionPayment = _earnings?.Episodes.FirstOrDefault()?.EarningsProfile.CompletionPayment;
        var fundingBandMax = _apprenticeship.Episodes.First().Prices.FirstOrDefault()?.FundingBandMaximum;
        var instalmentAmount =
            _earnings?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.FirstOrDefault()?.Amount;
        var currentPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];

        int daysInLearning =
            1 + (apprenticeshipCreatedEvent.EndDate.Date - apprenticeshipCreatedEvent.StartDate.Date).Days;

        int daysInLearningThisAY = 1 + (TokenisableDateTime.FromString("currentAY-07-31").Value - apprenticeshipCreatedEvent.StartDate.Date).Days; ;
        int plannedTotalDaysInLearning = 1 + (apprenticeshipCreatedEvent.EndDate.Date - apprenticeshipCreatedEvent.StartDate.Date).Days;
        var ageAtStartOfApprenticeship = _earnings?.Episodes.FirstOrDefault()?.AgeAtStartOfApprenticeship;
        var fundLineType = ageAtStartOfApprenticeship > 18 ? "19+ Apprenticeship (Employer on App Service)" : "16-18 Apprenticeship (Employer on App Service)";

        var firstIncentivePeriod = _earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DeliveryPeriod;
        var secondIncentivePeriod = _earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DeliveryPeriod;
        var firstIncentiveThresholdDate = _earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DueDate;
        var secondIncentiveThresholdDate = _earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DueDate;


        Assert.Multiple(() =>
        {
            Assert.AreEqual(EarningsFM36Constants.LearnRefNumber, fm36Learner.LearnRefNumber,
                "Unexpected Learner Ref Number found!");
            Assert.AreEqual(2, fm36Learner.EarningsPlatform, "Unexpected Earnings Platform found!");

            //Price Episodes
            Assert.AreEqual(expectedPriceEpisodeIdentifier, fm36Learner.PriceEpisodes.First().PriceEpisodeIdentifier, "Unexpected PriceEpisodeIdentifier found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.ActualStartDate, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.EpisodeStartDate, "Unexpected PriceEpisodeStartDate found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.PriceEpisodes.First().TrainingPrice, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.TNP1, "Unexpected TNP1 value found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.PriceEpisodes.First().EndPointAssessmentPrice, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.TNP2, "Unexpected TNP2 value found!");
            Assert.AreEqual(EarningsFM36Constants.TNP3, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.TNP3, "Unexpected TNP3 value found!");
            Assert.AreEqual(EarningsFM36Constants.TNP4, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.TNP4, "Unexpected TNP4 value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeActualEndDateIncEPA, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeActualEndDateIncEPA, "Unexpected PriceEpisodeActualEndDateIncEPA value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisode1618FUBalValue, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisode1618FUBalValue, "Unexpected PriceEpisode1618FUBalValue value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeApplic1618FrameworkUpliftCompElement, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeApplic1618FrameworkUpliftCompElement, "Unexpected priceEpisodeApplic1618FrameworkUpliftCompElement value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisode1618FrameworkUpliftTotPrevEarnings, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisode1618FrameworkUpliftTotPrevEarnings, "Unexpected priceEpisode1618FrameworkUpliftTotPrevEarnings value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisode1618FrameworkUpliftRemainingAmount, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisode1618FrameworkUpliftRemainingAmount, "Unexpected PriceEpisode1618FrameworkUpliftRemainingAmount value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisode1618FUMonthInstValue, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisode1618FUMonthInstValue, "Unexpected PriceEpisode1618FUMonthInstValue value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisode1618FUTotEarnings, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisode1618FUTotEarnings, "Unexpected PriceEpisode1618FUTotEarnings value found!");
            Assert.AreEqual(fundingBandMax, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeUpperBandLimit, "Unexpected PriceEpisodeUpperBandLimit value found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.EndDate, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodePlannedEndDate, "Unexpected PriceEpisodePlannedEndDate found!");
            Assert.AreEqual(null, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeActualEndDate, "Unexpected PriceEpisodeActualEndDate found!");
            Assert.AreEqual(totalPrice, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeTotalTNPPrice, "Unexpected PriceEpisodeTotalTNPPrice value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeUpperLimitAdjustment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeUpperLimitAdjustment, "Unexpected PriceEpisodeUpperLimitAdjustment value found!");
            Assert.AreEqual(_earnings?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.Count, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodePlannedInstalments, "Unexpected PriceEpisodePlannedInstalments value found!");
            Assert.AreEqual(0, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeActualInstalments, "Unexpected PriceEpisodeActualInstalments value found!");
            Assert.AreEqual(priceEpisodeInstalmentsThisPeriod, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeInstalmentsThisPeriod, "Unexpected PriceEpisodeInstalmentsThisPeriod value found!");
            Assert.AreEqual(completionPayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeCompletionElement, "Unexpected PriceEpisodeCompletionElement value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodePreviousEarnings, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodePreviousEarnings, "Unexpected PriceEpisodePreviousEarnings value found!");
            Assert.AreEqual(instalmentAmount, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeInstalmentValue, "Unexpected PriceEpisodeInstalmentValue value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeOnProgPayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeOnProgPayment, "Unexpected PriceEpisodeOnProgPayment value found!");
            Assert.AreEqual(onProgPayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeTotalEarnings, "Unexpected PriceEpisodeTotalEarnings value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeBalanceValue, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeBalanceValue, "Unexpected PriceEpisodeBalanceValue value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeBalancePayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeBalancePayment, "Unexpected PriceEpisodeBalancePayment value found!");
            Assert.AreEqual(false, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeCompleted, "Unexpected PriceEpisodeCompleted value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeCompletionPayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeCompletionPayment, "Unexpected PriceEpisodeCompletionPayment value found!");
            Assert.AreEqual(fundingBandMax - (instalmentAmount * (currentPeriod - 1)), fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeRemainingTNPAmount, "Unexpected PriceEpisodeRemainingTNPAmount value found!");
            Assert.AreEqual(fundingBandMax - (instalmentAmount * (currentPeriod - 1)), fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeRemainingAmountWithinUpperLimit, "Unexpected PriceEpisodeRemainingAmountWithinUpperLimit value found!");
            Assert.AreEqual(fundingBandMax - (instalmentAmount * (currentPeriod - 1)), fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeCappedRemainingTNPAmount, "Unexpected PriceEpisodeCappedRemainingTNPAmount value found!");
            Assert.AreEqual(fundingBandMax - (instalmentAmount * (currentPeriod - 1)) - completionPayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeExpectedTotalMonthlyValue, "Unexpected PriceEpisodeExpectedTotalMonthlyValue value found!");
            Assert.AreEqual(EarningsFM36Constants.AimSeqNumber, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeAimSeqNumber, "Unexpected PriceEpisodeAimSeqNumber value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeFirstDisadvantagePayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeFirstDisadvantagePayment, "Unexpected PriceEpisodeFirstDisadvantagePayment value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeSecondDisadvantagePayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeSecondDisadvantagePayment, "Unexpected PriceEpisodeSecondDisadvantagePayment value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeApplic1618FrameworkUpliftBalancing, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeApplic1618FrameworkUpliftBalancing, "Unexpected PriceEpisodeApplic1618FrameworkUpliftBalancing value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeApplic1618FrameworkUpliftCompletionPayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeApplic1618FrameworkUpliftCompletionPayment, "Unexpected PriceEpisodeApplic1618FrameworkUpliftCompletionPayment value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeApplic1618FrameworkUpliftOnProgPayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeApplic1618FrameworkUpliftOnProgPayment, "Unexpected PriceEpisodeApplic1618FrameworkUpliftOnProgPayment value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeSecondProv1618Pay, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeSecondProv1618Pay, "Unexpected PriceEpisodeSecondProv1618Pay value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeFirstEmp1618Pay, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeFirstEmp1618Pay, "Unexpected PriceEpisodeFirstEmp1618Pay value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeSecondEmp1618Pay, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeSecondEmp1618Pay, "Unexpected PriceEpisodeSecondEmp1618Pay value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeFirstProv1618Pay, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeFirstProv1618Pay, "Unexpected PriceEpisodeFirstProv1618Pay value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeLSFCash, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeLSFCash, "Unexpected PriceEpisodeLSFCash value found!");
            Assert.AreEqual(fundLineType, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeFundLineType, "Unexpected PriceEpisodeFundLineType value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeLevyNonPayInd, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeLevyNonPayInd, "Unexpected PriceEpisodeLevyNonPayInd value found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.ActualStartDate, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.EpisodeEffectiveTNPStartDate, "Unexpected EpisodeEffectiveTNPStartDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeFirstAdditionalPaymentThresholdDate, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeFirstAdditionalPaymentThresholdDate, "Unexpected PriceEpisodeFirstAdditionalPaymentThresholdDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeSecondAdditionalPaymentThresholdDate, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeSecondAdditionalPaymentThresholdDate, "Unexpected PriceEpisodeSecondAdditionalPaymentThresholdDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeContractType, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeContractType, "Unexpected PriceEpisodeContractType value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodePreviousEarningsSameProvider, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodePreviousEarningsSameProvider, "Unexpected PriceEpisodePreviousEarningsSameProvider value found!");
            Assert.AreEqual(onProgPayment, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeTotProgFunding, "Unexpected PriceEpisodeTotProgFunding value found!");
            Assert.AreEqual((onProgPayment * 95) / 100, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeProgFundIndMinCoInvest, "Unexpected PriceEpisodeProgFundIndMinCoInvest value found!");
            Assert.AreEqual((onProgPayment * 5) / 100, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeProgFundIndMaxEmpCont, "Unexpected PriceEpisodeProgFundIndMaxEmpCont value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeTotalPMRs, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeTotalPMRs, "Unexpected PriceEpisodeTotalPMRs value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeCumulativePMRs, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeCumulativePMRs, "Unexpected PriceEpisodeCumulativePMRs value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeCompExemCode, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeCompExemCode, "Unexpected PriceEpisodeCompExemCode value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeLearnerAdditionalPaymentThresholdDate, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeLearnerAdditionalPaymentThresholdDate, "Unexpected PriceEpisodeLearnerAdditionalPaymentThresholdDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeRedStartDate, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeRedStartDate, "Unexpected PriceEpisodeRedStartDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeRedStatusCode, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeRedStatusCode, "Unexpected PriceEpisodeRedStatusCode value found!");
            Assert.AreEqual("25-" + apprenticeshipCreatedEvent.TrainingCode, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeLDAppIdent, "Unexpected PriceEpisodeLDAppIdent value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeAugmentedBandLimitFactor, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeAugmentedBandLimitFactor, "Unexpected PriceEpisodeAugmentedBandLimitFactor value found!");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames
                    .PriceEpisodeApplic1618FrameworkUpliftBalancing)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeApplic1618FrameworkUpliftBalancing} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames
                    .PriceEpisodeApplic1618FrameworkUpliftCompletionPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeApplic1618FrameworkUpliftCompletionPayment} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames
                    .PriceEpisodeApplic1618FrameworkUpliftOnProgPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeApplic1618FrameworkUpliftOnProgPayment} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalancePayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalancePayment} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalanceValue)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalanceValue} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeCompletionPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeCompletionPayment} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstDisadvantagePayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstDisadvantagePayment} are zero.");

            if (ageAtStartOfApprenticeship < 19)
            {
                fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
                .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay} value for period {firstIncentivePeriod} is not 500");

                fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
                .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay} value for period {firstIncentivePeriod} is not 500");
            }
            else
            {
                fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay} are zero.");

                fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay} are zero.");
            }

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLevyNonPayInd)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLevyNonPayInd} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLSFCash)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLSFCash} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondDisadvantagePayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondDisadvantagePayment} are zero.");

            if (ageAtStartOfApprenticeship < 19)
            {
                fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
               .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondEmp1618Pay).SingleOrDefault(x => x.PeriodNumber == secondIncentivePeriod).Value
               .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondEmp1618Pay} value for period {secondIncentivePeriod} is not 500");

                fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
               .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondProv1618Pay).SingleOrDefault(x => x.PeriodNumber == secondIncentivePeriod).Value
               .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondProv1618Pay} value for period {secondIncentivePeriod} is not 500");
            }
            else
            {
                fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondEmp1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondEmp1618Pay} are zero.");

                fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondProv1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondProv1618Pay} are zero.");
            }

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLearnerAdditionalPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLearnerAdditionalPayment} are zero.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeInstalmentsThisPeriod)
                .Should().OnlyContain(x => x.Value == 1,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeInstalmentsThisPeriod} are 1.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeOnProgPayment)
                .Should().OnlyContain(x => x.Value == instalmentAmount,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeOnProgPayment} are {instalmentAmount}.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeProgFundIndMaxEmpCont)
                .Should().OnlyContain(x => x.Value == (instalmentAmount * 5) / 100,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeProgFundIndMaxEmpCont} are {(instalmentAmount * 5) / 100}.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeProgFundIndMinCoInvest)
                .Should().OnlyContain(x => x.Value == (instalmentAmount * 95) / 100,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeProgFundIndMinCoInvest} are {(instalmentAmount * 95) / 100}.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeTotProgFunding)
                .Should().OnlyContain(x => x.Value == instalmentAmount,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeTotProgFunding} are {instalmentAmount}.");

            fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeESFAContribPct)
                .Should().OnlyContain(x => x.Value == 0.95m,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeESFAContribPct} are 0.95.");

            // Learning Deliveries
            Assert.AreEqual(EarningsFM36Constants.AimSeqNumber, fm36Learner.LearningDeliveries.First().AimSeqNumber, "Unexpected AimSeqNumber found!");
            Assert.AreEqual(EarningsFM36Constants.ActualDaysIL, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.ActualDaysIL, "Unexpected ActualDaysIL found!");
            Assert.AreEqual(null, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.ActualNumInstalm, "Unexpected ActualNumInstalm found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.ActualStartDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.AdjStartDate, "Unexpected AdjStartDate found!");
            Assert.AreEqual(_earnings.Episodes.FirstOrDefault()?.AgeAtStartOfApprenticeship, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.AgeAtProgStart, "Unexpected AgeAtProgStart found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.ActualStartDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.AppAdjLearnStartDate, "Unexpected AppAdjLearnStartDate found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.ActualStartDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.AppAdjLearnStartDateMatchPathway, "Unexpected AppAdjLearnStartDateMatchPathway found!");
            Assert.AreEqual(EarningsFM36Constants.ApplicCompDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.ApplicCompDate, "Unexpected ApplicCompDate found!");
            Assert.AreEqual(EarningsFM36Constants.CombinedAdjProp, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.CombinedAdjProp, "Unexpected CombinedAdjProp found!");
            Assert.AreEqual(EarningsFM36Constants.Completed, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.Completed, "Unexpected Completed found!");
            Assert.AreEqual(firstIncentiveThresholdDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.FirstIncentiveThresholdDate, "Unexpected FirstIncentiveThresholdDate found!");
            Assert.AreEqual(EarningsFM36Constants.FundStart, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.FundStart, "Unexpected FundStart found!");
            Assert.AreEqual(null, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.FworkCode, "Unexpected FworkCode found!");
            Assert.AreEqual(EarningsFM36Constants.LDApplic1618FrameworkUpliftTotalActEarnings, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LDApplic1618FrameworkUpliftTotalActEarnings, "Unexpected LDApplic1618FrameworkUpliftTotalActEarnings found!");
            Assert.AreEqual(EarningsFM36Constants.LearnAimRef, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnAimRef, "Unexpected LearnAimRef found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.ActualStartDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnStartDate, "Unexpected LearnStartDate found!");
            Assert.AreEqual(ageAtStartOfApprenticeship < 19 ? true : false, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDel1618AtStart, "Unexpected LearnDel1618AtStart found!");
            Assert.AreEqual(daysInLearningThisAY, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelAppAccDaysIL, "Unexpected LearnDelAppAccDaysIL found!");
            Assert.AreEqual(daysInLearningThisAY, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelAppAccDaysIL, "Unexpected LearnDelAppAccDaysIL found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelApplicDisadvAmount, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicDisadvAmount, "Unexpected LearnDelApplicDisadvAmount found!");
            Assert.AreEqual(ageAtStartOfApprenticeship < 19 ? 1000 : EarningsFM36Constants.LearnDelApplicEmp1618Incentive, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicEmp1618Incentive, "Unexpected LearnDelApplicEmp1618Incentive found!");
            Assert.AreEqual(null, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicEmpDate, "Unexpected LearnDelApplicEmpDate found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelApplicProv1618FrameworkUplift, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicProv1618FrameworkUplift, "Unexpected LearnDelApplicProv1618FrameworkUplift found!");
            Assert.AreEqual(ageAtStartOfApprenticeship < 19 ? 1000 : EarningsFM36Constants.LearnDelApplicProv1618Incentive, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicProv1618Incentive, "Unexpected LearnDelApplicProv1618Incentive found!");
            Assert.AreEqual(daysInLearningThisAY, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelAppPrevAccDaysIL, "Unexpected LearnDelAppPrevAccDaysIL found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelDisadAmount, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelDisadAmount, "Unexpected LearnDelDisadAmount found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelEligDisadvPayment, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelEligDisadvPayment, "Unexpected LearnDelEligDisadvPayment found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelEmpIdFirstAdditionalPaymentThreshold, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelEmpIdFirstAdditionalPaymentThreshold, "Unexpected LearnDelEmpIdFirstAdditionalPaymentThreshold found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelEmpIdSecondAdditionalPaymentThreshold, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelEmpIdSecondAdditionalPaymentThreshold, "Unexpected LearnDelEmpIdSecondAdditionalPaymentThreshold found!");
            Assert.AreEqual(daysInLearningThisAY, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelHistDaysThisApp, "Unexpected LearnDelHistDaysThisApp found!");
            Assert.AreEqual(12 * instalmentAmount, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelHistProgEarnings, "Unexpected LearnDelHistProgEarnings found!");
            Assert.AreEqual(fundLineType, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelInitialFundLineType, "Unexpected LearnDelInitialFundLineType found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelProgEarliestACT2Date, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelProgEarliestACT2Date, "Unexpected LearnDelProgEarliestACT2Date found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelNonLevyProcured, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelNonLevyProcured, "Unexpected LearnDelNonLevyProcured found!");
            Assert.AreEqual(EarningsFM36Constants.MathEngAimValue, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.MathEngAimValue, "Unexpected MathEngAimValue found!");
            Assert.AreEqual(EarningsFM36Constants.OutstandNumOnProgInstalm, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.OutstandNumOnProgInstalm, "Unexpected OutstandNumOnProgInstalm found!");
            Assert.AreEqual(_earnings?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.Count, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.PlannedNumOnProgInstalm, "Unexpected PlannedNumOnProgInstalm found!");
            Assert.AreEqual(plannedTotalDaysInLearning, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.PlannedTotalDaysIL, "Unexpected PlannedTotalDaysIL found!");
            Assert.AreEqual(EarningsFM36Constants.ProgType, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.ProgType, "Unexpected ProgType found!");
            Assert.AreEqual(EarningsFM36Constants.PwayCode, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.PwayCode, "Unexpected PwayCode found!");
            Assert.AreEqual(secondIncentiveThresholdDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.SecondIncentiveThresholdDate, "Unexpected SecondIncentiveThresholdDate found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.TrainingCode, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.StdCode.ToString(), "Unexpected StdCode found!");
            Assert.AreEqual(EarningsFM36Constants.ThresholdDays, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.ThresholdDays, "Unexpected ThresholdDays found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelApplicCareLeaverIncentive, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicCareLeaverIncentive, "Unexpected LearnDelApplicCareLeaverIncentive found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelHistDaysCareLeavers, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelHistDaysCareLeavers, "Unexpected LearnDelHistDaysCareLeavers found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelAccDaysILCareLeavers, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelAccDaysILCareLeavers, "Unexpected LearnDelAccDaysILCareLeavers found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelPrevAccDaysILCareLeavers, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelPrevAccDaysILCareLeavers, "Unexpected LearnDelPrevAccDaysILCareLeavers found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelLearnerAddPayThresholdDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelLearnerAddPayThresholdDate, "Unexpected LearnDelLearnerAddPayThresholdDate found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelRedCode, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelRedCode, "Unexpected LearnDelRedCode found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelRedStartDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelRedStartDate, "Unexpected LearnDelRedStartDate found!");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.DisadvFirstPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.DisadvFirstPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.DisadvSecondPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.DisadvSecondPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.InstPerPeriod)
                .Should().OnlyContain(x => x.Value == 1,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.InstPerPeriod} are 1.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames
                    .LDApplic1618FrameworkUpliftBalancingPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LDApplic1618FrameworkUpliftBalancingPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames
                    .LDApplic1618FrameworkUpliftCompletionPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LDApplic1618FrameworkUpliftCompletionPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames
                    .LDApplic1618FrameworkUpliftOnProgPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LDApplic1618FrameworkUpliftOnProgPayment} are 0.");

            if (ageAtStartOfApprenticeship < 19)
            {
                fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstEmp1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
                .Should().Be(500, $"{LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstEmp1618Pay} value for period {firstIncentivePeriod} is not 500");

                fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                  .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstProv1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
                  .Should().Be(500, $"{LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstProv1618Pay} value for period {firstIncentivePeriod} is not 500");

            }
            else
            {
                fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstEmp1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstEmp1618Pay} are 0.");

                fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
               .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstProv1618Pay)
               .Should().OnlyContain(x => x.Value == 0, $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstProv1618Pay} are 0.");

            }

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.DisadvSecondPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.DisadvSecondPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelLearnAddPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelLearnAddPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelLevyNonPayInd)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelLevyNonPayInd} are 0.");

            if (ageAtStartOfApprenticeship < 19)
            {
                fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSecondEmp1618Pay).SingleOrDefault(x => x.PeriodNumber == secondIncentivePeriod).Value
                .Should().Be(500, $"{LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSecondEmp1618Pay} value for period {secondIncentivePeriod} is not 500");

                fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSecondProv1618Pay).SingleOrDefault(x => x.PeriodNumber == secondIncentivePeriod).Value
                .Should().Be(500, $"{LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSecondProv1618Pay} value for period {secondIncentivePeriod} is not 500");
            }
            else
            {
                fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSecondEmp1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSecondEmp1618Pay} are 0.");

                fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
               .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSecondProv1618Pay)
               .Should().OnlyContain(x => x.Value == 0, $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSecondProv1618Pay} are 0.");
            }

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSEMContWaiver)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelSEMContWaiver} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelESFAContribPct)
                .Should().OnlyContain(x => x.Value == 0.95m,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelESFAContribPct} are 0.95.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFund)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFund} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFundCash)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFundCash} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.MathEngBalPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.MathEngBalPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.MathEngOnProgPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.MathEngOnProgPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimBalPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimBalPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimCompletionPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimCompletionPayment} are 0.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimOnProgPayment)
                .Should().OnlyContain(x => x.Value == instalmentAmount,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimOnProgPayment} are {instalmentAmount}.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimProgFundIndMaxEmpCont)
                .Should().OnlyContain(x => x.Value == (instalmentAmount * 5) / 100,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimProgFundIndMaxEmpCont} are {(instalmentAmount * 5) / 100}.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(
                    LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimProgFundIndMinCoInvest)
                .Should().OnlyContain(x => x.Value == (instalmentAmount * 95) / 100,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimProgFundIndMinCoInvest} are {(instalmentAmount * 95) / 100}.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimTotProgFund)
                .Should().OnlyContain(x => x.Value == instalmentAmount,
                    $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.ProgrammeAimTotProgFund} are {instalmentAmount}.");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedTextValues
            .GetValuesForAttribute(LearningDeliveryPeriodisedTextValuesAttributeNames.FundLineType)
            .Should().OnlyContain(x => x.Value.ToString() == fundLineType, $"Not all {LearningDeliveryPeriodisedTextValuesAttributeNames.FundLineType} are 16-18 Apprenticeship (Employer on App Service).");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedTextValues
            .GetValuesForAttribute(LearningDeliveryPeriodisedTextValuesAttributeNames.LearnDelContType)
            .Should().OnlyContain(x => x.Value.ToString() == "Contract for services with the employer", $"Not all {LearningDeliveryPeriodisedTextValuesAttributeNames.LearnDelContType} are \"Contract for services with the employer\".");
        });
    }

    [Then(@"fm36 data does not exist for that apprenticeship")]
    public async Task Fm36DataDoesNotExist()
    {
        var fm36 = _context.Get<List<FM36Learner>>();
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var earningsSqlClient = new EarningsSqlClient();

        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        _apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);
        _earnings = earningsSqlClient.GetEarningsEntityModel(_context);

        // get your learner data 

        var fm36Learner = fm36.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);

        fm36Learner.Should().BeNull();
    }

    [Then(@"fm36 FundStart value is (.*)")]
    public void ThenFm36FundStartValueIsFalse(bool expectedValue)
    {
        var fm36 = _context.Get<List<FM36Learner>>();
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var earningsSqlClient = new EarningsSqlClient();

        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        _apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);
        _earnings = earningsSqlClient.GetEarningsEntityModel(_context);

        // get your learner data 

        var fm36Learner = fm36.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);

        Assert.AreEqual(expectedValue,
            fm36Learner.LearningDeliveries.First().LearningDeliveryValues.FundStart, "Unexpected FundStart found!");
    }

    [Then(@"fm36 ThresholdDays value is (.*)")]
    public void ThenFm36ThresholdDaysValueIs(int expectedValue)
    {
        var fm36 = _context.Get<List<FM36Learner>>();
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var earningsSqlClient = new EarningsSqlClient();

        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        _apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);
        _earnings = earningsSqlClient.GetEarningsEntityModel(_context);

        // get your learner data 

        var fm36Learner = fm36.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);

        Assert.AreEqual(expectedValue,
            fm36Learner.LearningDeliveries.First().LearningDeliveryValues.ThresholdDays, "Unexpected ThresholdDays value found!");
    }

    [Then(@"fm36 ActualDaysIL value is (.*)")]
    public void ThenFm36ActualDaysInLearningValueIs(int expectedValue)
    {
        var fm36 = _context.Get<List<FM36Learner>>();
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var earningsSqlClient = new EarningsSqlClient();

        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        _apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);
        _earnings = earningsSqlClient.GetEarningsEntityModel(_context);

        // get your learner data 

        var fm36Learner = fm36.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);

        Assert.AreEqual(expectedValue,
            fm36Learner.LearningDeliveries.First().LearningDeliveryValues.ActualDaysIL, "Unexpected FundStart found!");
    }

    [Then(@"fm36 block contains a new price episode starting (.*) with episode 1 tnp of (.*) and episode 2 tnp of (.*)")]
    public void ThenFm36BlockContainsANewPriceEpisodeStarting(TokenisableDateTime newEpisodeStartDate, decimal expectedEpisode1Tnp, decimal expectedEpisode2Tnp)
    {
        // Retrieve necessary data from the context
        var fm36Learners = _context.Get<List<FM36Learner>>();
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
        var apprenticeshipKey = _context.Get<Guid>(ContextKeys.ApprenticeshipKey);

        // Fetch apprenticeship and earnings data
        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        var earningsSqlClient = new EarningsSqlClient();

        _apprenticeship = apprenticeshipSqlClient.GetApprenticeship(apprenticeshipKey);
        _earnings = earningsSqlClient.GetEarningsEntityModel(_context);

        // Get the learner associated with the apprenticeship
        var fm36Learner = fm36Learners.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);
        Assert.IsNotNull(fm36Learner, $"Expected FM36 learner ULN {apprenticeshipCreatedEvent.Uln} not found.");

        // Ensure there are exactly 2 price episodes
        Assert.AreEqual(2, fm36Learner.PriceEpisodes.Count,
            $"Expected 2 price episodes but found {fm36Learner?.PriceEpisodes?.Count}.");

        // Extract price episodes
        var episode1 = fm36Learner.PriceEpisodes[0];
        var episode2 = fm36Learner.PriceEpisodes[1];

        // Validate Episode 1
        Assert.IsTrue(episode1.PriceEpisodeValues.PriceEpisodeCompleted,
            "Price Episode 1 was expected to be completed but was not.");

        var episode1Tnp1Expected = expectedEpisode1Tnp * 0.8m;
        var episode1Tnp2Expected = expectedEpisode1Tnp * 0.2m;

        Assert.AreEqual(episode1Tnp1Expected, episode1.PriceEpisodeValues.TNP1,
            $"Episode 1 TNP1 mismatch. Expected: {episode1Tnp1Expected}, Actual: {episode1.PriceEpisodeValues.TNP1}");
        Assert.AreEqual(episode1Tnp2Expected, episode1.PriceEpisodeValues.TNP2,
            $"Episode 1 TNP2 mismatch. Expected: {episode1Tnp2Expected}, Actual: {episode1.PriceEpisodeValues.TNP2}");

        // Validate Episode 2
        Assert.AreEqual(newEpisodeStartDate.Value.Date, episode2.PriceEpisodeValues.EpisodeStartDate,
            $"Episode 2 start date mismatch. Expected: {newEpisodeStartDate.Value.Date}, Actual: {episode2.PriceEpisodeValues.EpisodeStartDate}");

        var episode2Tnp1Expected = expectedEpisode2Tnp * 0.8m;
        var episode2Tnp2Expected = expectedEpisode2Tnp * 0.2m;

        Assert.AreEqual(episode2Tnp1Expected, episode2.PriceEpisodeValues.TNP1,
            $"Episode 2 TNP1 mismatch. Expected: {episode2Tnp1Expected}, Actual: {episode2.PriceEpisodeValues.TNP1}");
        Assert.AreEqual(episode2Tnp2Expected, episode2.PriceEpisodeValues.TNP2,
            $"Episode 2 TNP2 mismatch. Expected: {episode2Tnp2Expected}, Actual: {episode2.PriceEpisodeValues.TNP2}");
    }

    [Then(@"the fm36 PriceEpisodeInstalmentValue is (.*)")]
    public void ThenTheFmPriceEpisodeInstalmentValueIs(int priceEpisodeInstalmentValue)
    {
        var fm36 = _context.Get<List<FM36Learner>>();
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
        var fm36Learner = fm36.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);
        Assert.AreEqual(priceEpisodeInstalmentValue, fm36Learner!.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeInstalmentValue);
    }
}