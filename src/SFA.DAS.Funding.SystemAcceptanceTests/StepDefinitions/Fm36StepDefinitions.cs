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

    [When(@"the fm36 data is retrieved for (.*)")]
    public async Task GetFm36Data(TokenisableDateTime searchDate)
    {
        var apprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();
        var collectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", searchDate.Value));
        var collectionPeriod = TableExtensions.Period[searchDate.Value.ToString("MMMM")];

        var fm36 = await _earningsOuterClient.GetFm36Block(apprenticeshipCreatedEvent.ProviderId, collectionYear, collectionPeriod);
        _context.Set(fm36);
    }

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

        var expectedPriceEpisodeIdentifier = "25-" + apprenticeshipCreatedEvent.TrainingCode + "-" + apprenticeshipCreatedEvent.ActualStartDate?.ToString("dd/MM/yyyy");
        var priceEpisodeInstalmentsThisPeriod = (DateTime.Today >= apprenticeshipCreatedEvent.ActualStartDate && DateTime.Today <= apprenticeshipCreatedEvent.EndDate) ? 1 : 0;
        var totalPrice = _earnings?.Episodes.FirstOrDefault()?.Prices.FirstOrDefault()?.AgreedPrice;
        var onProgPayment = _earnings?.Episodes.FirstOrDefault()?.EarningsProfile.OnProgramTotal;
        var completionPayment = _earnings?.Episodes.FirstOrDefault()?.EarningsProfile.CompletionPayment;
        var fundingBandMax = _apprenticeship.Episodes.First().Prices.FirstOrDefault()?.FundingBandMaximum;
        var instalmentAmount = _earnings?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.FirstOrDefault()?.Amount;


        var currentPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];

        Assert.Multiple(() =>
        {
            Assert.AreEqual(EarningsFM36Constants.LearnRefNumber, fm36Learner.LearnRefNumber, "Unexpected Learner Ref Number found!");
            Assert.AreEqual(2, fm36Learner.EarningsPlatform, "Unexpected Earnings Platform found!");

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
            Assert.AreEqual("16-18 Apprenticeship (Employer on App Service)", fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeFundLineType, "Unexpected PriceEpisodeFundLineType value found!");
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
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeApplic1618FrameworkUpliftBalancing)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeApplic1618FrameworkUpliftBalancing} are zero.");

            var PriceEpisodeApplic1618FrameworkUpliftCompletionPayment = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeApplic1618FrameworkUpliftBalancing").FirstOrDefault();
            var allZeroPriceEpisodeApplic1618FrameworkUpliftCompletionPayment = PriceEpisodeApplic1618FrameworkUpliftCompletionPayment?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeApplic1618FrameworkUpliftCompletionPayment) == 0);
            Assert.IsTrue(allZeroPriceEpisodeApplic1618FrameworkUpliftCompletionPayment, "Not all priceEpisodeApplic1618FrameworkUpliftCompletionPayment are zero.");

            var PriceEpisodeApplic1618FrameworkUpliftOnProgPayment = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeApplic1618FrameworkUpliftOnProgPayment").FirstOrDefault();
            var allZeroPriceEpisodeApplic1618FrameworkUpliftOnProgPayment = PriceEpisodeApplic1618FrameworkUpliftOnProgPayment?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeApplic1618FrameworkUpliftOnProgPayment) == 0);
            Assert.IsTrue(allZeroPriceEpisodeApplic1618FrameworkUpliftOnProgPayment, "Not all PriceEpisodeApplic1618FrameworkUpliftOnProgPayment are zero.");

            var PriceEpisodeBalancePayment = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeBalancePayment").FirstOrDefault();
            var allZeroPriceEpisodeBalancePayment = PriceEpisodeBalancePayment?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeBalancePayment) == 0);
            Assert.IsTrue(allZeroPriceEpisodeBalancePayment, "Not all PriceEpisodeBalancePayment are zero.");

            var PriceEpisodeBalanceValue = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeBalanceValue").FirstOrDefault();
            var allZeroPriceEpisodeBalanceValue = PriceEpisodeBalanceValue?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeBalanceValue) == 0);
            Assert.IsTrue(allZeroPriceEpisodeBalanceValue, "Not all PriceEpisodeBalanceValue are zero.");

            var PriceEpisodeCompletionPayment = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeCompletionPayment").FirstOrDefault();
            var allZeroPriceEpisodeCompletionPayment = PriceEpisodeCompletionPayment?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeCompletionPayment) == 0);
            Assert.IsTrue(allZeroPriceEpisodeCompletionPayment, "Not all PriceEpisodeCompletionPayment are zero.");

            var PriceEpisodeFirstDisadvantagePayment = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeFirstDisadvantagePayment").FirstOrDefault();
            var allZeroPriceEpisodeFirstDisadvantagePayment = PriceEpisodeFirstDisadvantagePayment?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeFirstDisadvantagePayment) == 0);
            Assert.IsTrue(allZeroPriceEpisodeFirstDisadvantagePayment, "Not all PriceEpisodeFirstDisadvantagePayment are zero.");

            var PriceEpisodeFirstEmp1618Pay = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeFirstEmp1618Pay").FirstOrDefault();
            var allZeroPriceEpisodeFirstEmp1618Pay = PriceEpisodeFirstEmp1618Pay?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeFirstEmp1618Pay) == 0);
            Assert.IsTrue(allZeroPriceEpisodeFirstEmp1618Pay, "Not all PriceEpisodeFirstEmp1618Pay are zero.");

            var PriceEpisodeFirstProv1618Pay = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeFirstProv1618Pay").FirstOrDefault();
            var allZeroPriceEpisodeFirstProv1618Pay = PriceEpisodeFirstProv1618Pay?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeFirstProv1618Pay) == 0);
            Assert.IsTrue(allZeroPriceEpisodeFirstProv1618Pay, "Not all PriceEpisodeFirstProv1618Pay are zero.");

            var PriceEpisodeLevyNonPayInd = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeLevyNonPayInd").FirstOrDefault();
            var allZeroPriceEpisodeLevyNonPayInd = PriceEpisodeLevyNonPayInd?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeLevyNonPayInd) == 0);
            Assert.IsTrue(allZeroPriceEpisodeLevyNonPayInd, "Not all PriceEpisodeLevyNonPayInd are zero.");

            var PriceEpisodeLSFCash = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeLSFCash").FirstOrDefault();
            var allZeroPriceEpisodeLSFCash = PriceEpisodeLSFCash?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeLSFCash) == 0);
            Assert.IsTrue(allZeroPriceEpisodeLSFCash, "Not all PriceEpisodeLSFCash are zero.");

            var PriceEpisodeSecondDisadvantagePayment = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeSecondDisadvantagePayment").FirstOrDefault();
            var allZeroPriceEpisodeSecondDisadvantagePayment = PriceEpisodeSecondDisadvantagePayment?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeSecondDisadvantagePayment) == 0);
            Assert.IsTrue(allZeroPriceEpisodeSecondDisadvantagePayment, "Not all PriceEpisodeSecondDisadvantagePayment are zero.");

            var PriceEpisodeSecondEmp1618Pay = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeSecondEmp1618Pay").FirstOrDefault();
            var allZeroPriceEpisodeSecondEmp1618Pay = PriceEpisodeSecondEmp1618Pay?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeSecondEmp1618Pay) == 0);
            Assert.IsTrue(allZeroPriceEpisodeSecondEmp1618Pay, "Not all PriceEpisodeSecondEmp1618Pay are zero.");

            var PriceEpisodeSecondProv1618Pay = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeSecondProv1618Pay").FirstOrDefault();
            var allZeroPriceEpisodeSecondProv1618Pay = PriceEpisodeSecondProv1618Pay?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeSecondProv1618Pay) == 0);
            Assert.IsTrue(allZeroPriceEpisodeSecondProv1618Pay, "Not all PriceEpisodeSecondProv1618Pay are zero.");

            var PriceEpisodeLearnerAdditionalPayment = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeLearnerAdditionalPayment").FirstOrDefault();
            var allZeroPriceEpisodeLearnerAdditionalPayment = PriceEpisodeLearnerAdditionalPayment?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeLearnerAdditionalPayment) == 0);
            Assert.IsTrue(allZeroPriceEpisodeLearnerAdditionalPayment, "Not all PriceEpisodeLearnerAdditionalPayment are zero.");

            var PriceEpisodeInstalmentsThisPeriod = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeInstalmentsThisPeriod").FirstOrDefault();
            var allOnePriceEpisodeInstalmentsThisPeriod = PriceEpisodeInstalmentsThisPeriod?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeInstalmentsThisPeriod) == 1);
            Assert.IsTrue(allOnePriceEpisodeInstalmentsThisPeriod, "Not all PriceEpisodeLearnerAdditionalPayment are 1.");

            var PriceEpisodeOnProgPayment = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeOnProgPayment").FirstOrDefault();
            var allZeroPriceEpisodeOnProgPayment = PriceEpisodeOnProgPayment?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeOnProgPayment) == instalmentAmount);
            Assert.IsTrue(allZeroPriceEpisodeOnProgPayment, $"Not all PriceEpisodeOnProgPayment are {instalmentAmount}.");

            var PriceEpisodeProgFundIndMaxEmpCont = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeProgFundIndMaxEmpCont").FirstOrDefault();
            var allCorrectPriceEpisodeProgFundIndMaxEmpCont = PriceEpisodeProgFundIndMaxEmpCont?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeProgFundIndMaxEmpCont) == (instalmentAmount*5)/100);
            Assert.IsTrue(allCorrectPriceEpisodeProgFundIndMaxEmpCont, $"Not all PriceEpisodeProgFundIndMaxEmpCont are {(instalmentAmount * 5) / 100}.");

            var PriceEpisodeProgFundIndMinCoInvest = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeProgFundIndMinCoInvest").FirstOrDefault();
            var allCorrectPriceEpisodeProgFundIndMinCoInvest = PriceEpisodeProgFundIndMinCoInvest?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeProgFundIndMinCoInvest) == (instalmentAmount * 95) / 100);
            Assert.IsTrue(allCorrectPriceEpisodeProgFundIndMinCoInvest, $"Not all PriceEpisodeProgFundIndMinCoInvest are {(instalmentAmount * 95) / 100}.");

            var PriceEpisodeTotProgFunding = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeTotProgFunding").FirstOrDefault();
            var allCorrectPriceEpisodeTotProgFunding = PriceEpisodeTotProgFunding?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeTotProgFunding) == instalmentAmount);
            Assert.IsTrue(allCorrectPriceEpisodeTotProgFunding, $"Not all PriceEpisodeTotProgFunding are {instalmentAmount}");

            var PriceEpisodeESFAContribPct = fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues.Where(x => x.AttributeName == "PriceEpisodeESFAContribPct").FirstOrDefault();
            var allCorrectPriceEpisodeESFAContribPct = PriceEpisodeESFAContribPct?.GetType().GetProperties().Where(p => p.Name.StartsWith("period")).All(p => (int)p.GetValue(PriceEpisodeESFAContribPct) == instalmentAmount);
            Assert.IsTrue(allCorrectPriceEpisodeESFAContribPct, "Not all PriceEpisodeESFAContribPct are 0.95");

        });
    }
}
