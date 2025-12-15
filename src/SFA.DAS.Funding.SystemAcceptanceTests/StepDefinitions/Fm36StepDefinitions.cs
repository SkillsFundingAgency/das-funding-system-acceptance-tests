using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Newtonsoft.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Learning.Types;
using System.Collections.Generic;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class Fm36StepDefinitions
{
    private readonly ScenarioContext _context;
    private readonly EarningsOuterClient _earningsOuterClient;
    private readonly LearnerDataOuterApiClient _learnerDataOuterApiClient;
    private readonly LearningSqlClient _apprenticeshipSqlClient;
    private readonly EarningsSqlClient _earningsSqlClient;
    private readonly EarningsInnerApiHelper _earningsInnerApiHelper;

    public Fm36StepDefinitions(
        ScenarioContext context,
        EarningsOuterClient earningsOuterClient,
        LearnerDataOuterApiClient learnerDataOuterApiClient,
        LearningSqlClient apprenticeshipSqlClient,
        EarningsSqlClient earningsSqlClient,
        EarningsInnerApiHelper earningsInnerApiHelper)
    {
        _context = context;
        _earningsOuterClient = earningsOuterClient;
        _learnerDataOuterApiClient = learnerDataOuterApiClient;
        _apprenticeshipSqlClient = apprenticeshipSqlClient;
        _earningsSqlClient = earningsSqlClient;
        _earningsInnerApiHelper = earningsInnerApiHelper;
    }

    [Given(@"the fm36 data is retrieved for (.*)")]
    [When(@"the fm36 data is retrieved for (.*)")]
    [Then(@"the fm36 data is retrieved for (.*)")]
    public async Task GetFm36Data(TokenisableDateTime searchDate)
    {
        var testData = _context.Get<TestData>();
        var collectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", searchDate.Value));
        var collectionPeriod = TableExtensions.Period[searchDate.Value.ToString("MMMM")];

        testData.FM36Learners = await _learnerDataOuterApiClient.GetFm36Block(testData.CommitmentsApprenticeshipCreatedEvent.ProviderId, collectionYear,
            collectionPeriod);

    }

    [Given(@"that there is at least 15 records available from FM36 endpoint")]
    public async Task GivenThatRecordsExistInFm36Endpoint()
    {
        // The purpose of this endpoint is to ensure paging tests can be run. There should be
        // at least 15 records of fm36 data from previous tests. If not then we create some test records here.
        // the content of the records is not important for paging tests.

        var testData = _context.Get<TestData>();
        var now = DateTime.Now;
        var collectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", now));
        var collectionPeriod = TableExtensions.Period[now.ToString("MMMM")];

        var existingFm36Records = await _learnerDataOuterApiClient.GetFm36Block(Constants.UkPrn, collectionYear, collectionPeriod);

        if(existingFm36Records.Count >= 15)
            return;

        var recordsToCreate = 15 - existingFm36Records.Count;

        for (int i = 0; i < recordsToCreate; i++)
        {
            var startDate = TokenisableDateTime.FromString("currentAY-08-23");
            var plannedEndDate = TokenisableDateTime.FromString("currentAYPlusTwo-08-23");
            testData.CommitmentsApprenticeshipCreatedEvent = _context.CreateApprenticeshipCreatedMessageWithCustomValues(startDate.Value, plannedEndDate.Value, 15000, "2");
            await _context.PublishApprenticeshipApprovedMessage(testData.CommitmentsApprenticeshipCreatedEvent);
        }
    }

    [When("the fm36 data is retrieved through LearnerData outer api for (.*)")]
    public async Task WhenTheFmDataIsRetrievedThroughLearnerDataOuterApiForCurrentDate(TokenisableDateTime searchDate)
    {
        var testData = _context.Get<TestData>();
        var collectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", searchDate.Value));
        var collectionPeriod = TableExtensions.Period[searchDate.Value.ToString("MMMM")];

        testData.FM36Learners = await _learnerDataOuterApiClient.GetFm36Block(testData.CommitmentsApprenticeshipCreatedEvent.ProviderId, collectionYear,
            collectionPeriod);
    }

    [When(@"a request is made without paging parameters")]
    public async Task WhenARequestIsMadeWithoutPagingParameters()
    {
        var testData = _context.Get<TestData>();
        var now = DateTime.Now;
        var collectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", now));
        var collectionPeriod = TableExtensions.Period[now.ToString("MMMM")];
        testData.Fm36HttpResponseMessage = await _learnerDataOuterApiClient.GetFm36BlockHttpResponseMessage(Constants.UkPrn, collectionYear, collectionPeriod);
    }

    [When(@"a request is made with page number (.*) and page size (.*)")]
    public async Task WhenARequestIsMadeWithPageNumberAndPageSize(int pageNumber, int pageSize)
    {
        var testData = _context.Get<TestData>();
        var now = DateTime.Now;
        var collectionYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", now));
        var collectionPeriod = TableExtensions.Period[now.ToString("MMMM")];
        testData.Fm36HttpResponseMessage = await _learnerDataOuterApiClient.GetFm36BlockHttpResponseMessage(Constants.UkPrn, collectionYear, collectionPeriod, pageSize, pageNumber);
    }

    [Given(@"the apprentice is marked as a care leaver")]
    public async Task GivenTheApprenticeIsMarkedAsACareLeaver()
    {
        var testData = _context.Get<TestData>();
        await _earningsInnerApiHelper.MarkAsCareLeaver(testData.LearningKey);
    }

    [Then("incentives earnings are generated for learners aged 15")]
    [Then(@"fm36 data exists for that apprenticeship")]
    public void Fm36DataExists()
    {
        var testData = _context.Get<TestData>();
        var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        var apprenticeship = _apprenticeshipSqlClient.GetApprenticeship(testData.LearningKey);
        var earnings = _earningsSqlClient.GetEarningsEntityModel(_context);
        if(earnings == null)
        {
            throw new Exception("Earnings data not found for the apprenticeship.");
        }

        // get your learner data 

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);
        if (fm36Learner == null)
        {
            throw new Exception($"No FM36 data found for ULN {apprenticeshipCreatedEvent.Uln}");
        }

        var expectedPriceEpisodeIdentifier = "25-" + apprenticeshipCreatedEvent.TrainingCode + "-" +
                                             apprenticeshipCreatedEvent.ActualStartDate?.ToString("dd/MM/yyyy");
        var priceEpisodeInstalmentsThisPeriod = (DateTime.Today >= apprenticeshipCreatedEvent.ActualStartDate &&
                                                 DateTime.Today <= apprenticeshipCreatedEvent.EndDate)
            ? 1
            : 0;
        var totalPrice = earnings?.Episodes.FirstOrDefault()?.Prices.FirstOrDefault()?.AgreedPrice;
        var onProgPayment = earnings?.Episodes.FirstOrDefault()?.EarningsProfile.OnProgramTotal;
        var completionPayment = earnings?.Episodes.FirstOrDefault()?.EarningsProfile.CompletionPayment;
        var fundingBandMax = apprenticeship.Episodes.First().Prices.FirstOrDefault()?.FundingBandMaximum;
        var instalmentAmount =
            earnings?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.FirstOrDefault()?.Amount;
        var currentPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];

        int daysInLearning =
            1 + (apprenticeshipCreatedEvent.EndDate.Date - apprenticeshipCreatedEvent.StartDate.Date).Days;

        int daysInLearningThisAY = 1 + (TokenisableDateTime.FromString("currentAY-07-31").Value - apprenticeshipCreatedEvent.StartDate.Date).Days; ;
        int plannedTotalDaysInLearning = 1 + (apprenticeshipCreatedEvent.EndDate.Date - apprenticeshipCreatedEvent.StartDate.Date).Days;
        var ageAtStartOfApprenticeship =  CalculateAgeAtStart(apprenticeshipCreatedEvent.StartDate, apprenticeshipCreatedEvent.DateOfBirth);
        var fundLineType = ageAtStartOfApprenticeship > 18 ? "19+ Apprenticeship (Employer on App Service)" : "16-18 Apprenticeship (Employer on App Service)";

        var firstIncentivePeriod = earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DeliveryPeriod;
        var secondIncentivePeriod = earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DeliveryPeriod;
        var firstIncentiveThresholdDate = earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DueDate;
        var secondIncentiveThresholdDate = earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DueDate;


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
            Assert.AreEqual(earnings?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.Count, fm36Learner.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodePlannedInstalments, "Unexpected PriceEpisodePlannedInstalments value found!");
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
            Assert.AreEqual(ageAtStartOfApprenticeship, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.AgeAtProgStart, "Unexpected AgeAtProgStart found!");
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
            Assert.AreEqual(earnings?.Episodes.FirstOrDefault()?.EarningsProfile.Instalments.Count, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.PlannedNumOnProgInstalm, "Unexpected PlannedNumOnProgInstalm found!");
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
    public void Fm36DataDoesNotExist()
    {
        var testData = _context.Get<TestData>();
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);
        fm36Learner.Should().BeNull();
    }

    [Then("learner is returned in the fm36 response")]
    public void ThenLearnerIsReturnedInTheFmResponse()
    {
        var testData = _context.Get<TestData>();
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);
        fm36Learner.Should().NotBeNull();
    }

    [Then(@"fm36 FundStart value is (.*)")]
    public void ThenFm36FundStartValueIsFalse(bool expectedValue)
    {
        var testData = _context.Get<TestData>();
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);
        Assert.AreEqual(expectedValue,
            fm36Learner!.LearningDeliveries.First().LearningDeliveryValues.FundStart, "Unexpected FundStart found!");
    }

    [Then(@"fm36 ThresholdDays value is (.*)")]
    public void ThenFm36ThresholdDaysValueIs(int expectedValue)
    {
        var testData = _context.Get<TestData>();
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        Assert.AreEqual(expectedValue,
            fm36Learner!.LearningDeliveries.First().LearningDeliveryValues.ThresholdDays, "Unexpected ThresholdDays value found!");
    }

    [Then(@"fm36 ActualDaysIL value is (.*)")]
    public void ThenFm36ActualDaysInLearningValue(int expectedValue)
    {
        var testData = _context.Get<TestData>();
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        Assert.AreEqual(expectedValue,
            fm36Learner!.LearningDeliveries.First().LearningDeliveryValues.ActualDaysIL, "Unexpected FundStart found!");
    }

    [Then("fm36 ActualEndDate value is (.*)")]
    public void Fm36ActualEndDateValueIs(TokenisableDateTime lastDayOfLearning)
    {
        var testData = _context.Get<TestData>();
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        Assert.AreEqual(lastDayOfLearning.Value,
            fm36Learner!.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeActualEndDate, "Unexpected PriceEpisodeActualEndDate found!");
    }

    [Then(@"fm36 block contains a new price episode starting (.*) with episode 1 tnp of (.*) and episode 2 tnp of (.*)")]
    public void ThenFm36BlockContainsANewPriceEpisodeStarting(TokenisableDateTime newEpisodeStartDate, decimal expectedEpisode1Tnp, decimal expectedEpisode2Tnp)
    {
        var testData = _context.Get<TestData>();

        // Retrieve necessary data from the context
        var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        // Fetch earnings data
        var earnings = _earningsSqlClient.GetEarningsEntityModel(_context);

        // Get the learner associated with the apprenticeship
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);
        Assert.IsNotNull(fm36Learner, $"Expected FM36 learner ULN {apprenticeshipCreatedEvent.Uln} not found.");

        // Ensure there are exactly 2 price episodes
        Assert.AreEqual(2, fm36Learner!.PriceEpisodes.Count,
            $"Expected 2 price episodes but found {fm36Learner?.PriceEpisodes?.Count}.");

        // Extract price episodes
        var episode1 = fm36Learner!.PriceEpisodes[0];
        var episode2 = fm36Learner.PriceEpisodes[1];

        // Validate Episode 1
        Assert.IsTrue(episode1.PriceEpisodeValues.PriceEpisodeCompleted,
            "Price Episode 1 was expected to be completed but was not.");

        var episode1Tnp1Expected = expectedEpisode1Tnp * 0.8m;
        var episode1Tnp2Expected = expectedEpisode1Tnp * 0.2m;
        var totalInstalments = earnings?.Episodes[0].EarningsProfile.Instalments.Count;

        Assert.AreEqual(episode1Tnp1Expected, episode1.PriceEpisodeValues.TNP1,
            $"Episode 1 TNP1 mismatch. Expected: {episode1Tnp1Expected}, Actual: {episode1.PriceEpisodeValues.TNP1}");
        Assert.AreEqual(episode1Tnp2Expected, episode1.PriceEpisodeValues.TNP2,
            $"Episode 1 TNP2 mismatch. Expected: {episode1Tnp2Expected}, Actual: {episode1.PriceEpisodeValues.TNP2}");
        Assert.AreEqual(episode1Tnp1Expected / totalInstalments, episode1.PriceEpisodeValues.PriceEpisodeTotalEarnings, "Incorrect PriceEpisodeTotalEarnings found for first price episode");

        // Validate Episode 2
        Assert.AreEqual(newEpisodeStartDate.Value.Date, episode2.PriceEpisodeValues.EpisodeStartDate,
            $"Episode 2 start date mismatch. Expected: {newEpisodeStartDate.Value.Date}, Actual: {episode2.PriceEpisodeValues.EpisodeStartDate}");

        var episode2Tnp1Expected = expectedEpisode2Tnp * 0.8m;
        var episode2Tnp2Expected = expectedEpisode2Tnp * 0.2m;

        Assert.AreEqual(episode2Tnp1Expected, episode2.PriceEpisodeValues.TNP1,
            $"Episode 2 TNP1 mismatch. Expected: {episode2Tnp1Expected}, Actual: {episode2.PriceEpisodeValues.TNP1}");
        Assert.AreEqual(episode2Tnp2Expected, episode2.PriceEpisodeValues.TNP2,
            $"Episode 2 TNP2 mismatch. Expected: {episode2Tnp2Expected}, Actual: {episode2.PriceEpisodeValues.TNP2}");

        var totalPaidInFirstPriceEpisode = episode1Tnp1Expected / totalInstalments;
        var totalToBePaidInSecondPriceEpisode = episode2Tnp1Expected - totalPaidInFirstPriceEpisode;

        Assert.AreEqual(Math.Round((decimal)totalToBePaidInSecondPriceEpisode!), Math.Round((decimal)episode2.PriceEpisodeValues.PriceEpisodeTotalEarnings!), "Incorrect PriceEpisodeTotalEarnings found for second price episode");

    }

    [Then(@"the fm36 PriceEpisodeInstalmentValue is (.*)")]
    public void ThenTheFmPriceEpisodeInstalmentValueIs(int priceEpisodeInstalmentValue)
    {
        var testData = _context.Get<TestData>();
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);
        Assert.AreEqual(priceEpisodeInstalmentValue, fm36Learner!.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeInstalmentValue);
    }

    [Then("Incentive periods and dates are updated in the fm36 response")]
    public void IncentivePeriodsAndDatesAreUpdatedInTheFm36Response()
    {
        var testData = _context.Get<TestData>();
        // learner has to be eligible for incentive earnings 
        var earnings = _earningsSqlClient.GetEarningsEntityModel(_context);

        // get your learner data 

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        var firstIncentivePeriod = earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DeliveryPeriod;
        var firstIncentiveThresholdDate = earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DueDate;
        var secondIncentivePeriod = earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DeliveryPeriod;
        var secondIncentiveThresholdDate = earnings?.Episodes.FirstOrDefault()?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DueDate;

        var expectedSecondIncentive = secondIncentivePeriod != firstIncentivePeriod;

        fm36Learner!.LearningDeliveries.First().LearningDeliveryValues.LearnDel1618AtStart.Should().BeTrue();
        fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicProv1618Incentive.Should().Be(expectedSecondIncentive ? 1000 : 500);
        fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicEmp1618Incentive.Should().Be(expectedSecondIncentive ? 1000 : 500);

        fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
            .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
            .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay} value for period {firstIncentivePeriod} is not 500");

        fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
            .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
            .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay} value for period {firstIncentivePeriod} is not 500");

        fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
            .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstEmp1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
            .Should().Be(500, $"{LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstEmp1618Pay} value for period {firstIncentivePeriod} is not 500");

        fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
            .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstProv1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
            .Should().Be(500, $"{LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstProv1618Pay} value for period {firstIncentivePeriod} is not 500");

        Assert.AreEqual(firstIncentiveThresholdDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.FirstIncentiveThresholdDate, "Unexpected FirstIncentiveThresholdDate found!");

        if (expectedSecondIncentive) Assert.AreEqual(secondIncentiveThresholdDate, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.SecondIncentiveThresholdDate, "Unexpected SecondIncentiveThresholdDate found!");
    }

    [Then("learning support amounts and periods from (.*) to (.*) are updated in the fm36 response")]
    public void LearningSupportAmountsAndPeriodsFromCurrentAY_RToCurrentAY_RAreUpdatedInTheFmResponse(TokenisablePeriod learningSupportStart, TokenisablePeriod learningSupportEnd)
    {
        var testData = _context.Get<TestData>();

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        for (var i = learningSupportStart.Value.PeriodValue; i <= learningSupportEnd.Value.PeriodValue; i++)
        {
            fm36Learner!.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
            .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLSFCash).SingleOrDefault(x => x.PeriodNumber == i).Value
            .Should().Be(150, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLSFCash} value for period {i} is not 150");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFund).SingleOrDefault(x => x.PeriodNumber == i).Value
                .Should().Be(1, $"{LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFund} value for period {i} is not 1");

            fm36Learner.LearningDeliveries.FirstOrDefault()?.LearningDeliveryPeriodisedValues
                .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFundCash).SingleOrDefault(x => x.PeriodNumber == i).Value
                .Should().Be(150, $"{LearningDeliveryPeriodisedValuesAttributeNames.LearnDelFirstProv1618Pay} value for period {i} is not 150");
        }
    }

    [Then("PriceEpisodeActualEndDateIncEPA is (.*)")]
    public void ValidatePriceEpisodeActualEndDateIncEPA(TokenisableDateTime? completionDate)
    {
        var testData = _context.Get<TestData>();

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodeValues.PriceEpisodeActualEndDateIncEPA
            .Should().Be(completionDate.Value, $"{EarningsFM36Constants.PriceEpisodeActualEndDateIncEPA} value is not {completionDate.Value}");
    }

    [Then("PriceEpisodeBalancePayment for period (.*) is amount (.*)")]
    public void ValidatePriceEpisodeBalancePayment(TokenisablePeriod? balancingPaymentPeriod, int amount)
    {
        var testData = _context.Get<TestData>();

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
            .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalancePayment).SingleOrDefault(x => x.PeriodNumber == balancingPaymentPeriod.Value.PeriodValue).Value
            .Should().Be(amount, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalancePayment} value for period {balancingPaymentPeriod.Value} is not {amount}");
    }

    [Then("PriceEpisodeCompletionPayment for period (.*) is amount (.*)")]
    public void ValidatePriceEpisodeCompletionPayment(TokenisablePeriod? completionPaymentPeriod, int amount)
    {
        var testData = _context.Get<TestData>();

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        fm36Learner.PriceEpisodes.FirstOrDefault()?.PriceEpisodePeriodisedValues
            .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeCompletionPayment).SingleOrDefault(x => x.PeriodNumber == completionPaymentPeriod.Value.PeriodValue).Value
            .Should().Be(amount, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeCompletionPayment} value for period {completionPaymentPeriod.Value} is not {amount}");
    }

    [Then(@"the response should be unpaged")]
    public async Task ThenTheResponseShouldBeUnpaged()
    {
        var testData = _context.Get<TestData>();
        var responseContent = await testData.Fm36HttpResponseMessage!.Content.ReadAsStringAsync();
        var learners = JsonConvert.DeserializeObject<List<FM36Learner>>(responseContent);

        Assert.IsNotNull(learners, "FM36 learners response is null");
        learners!.Count.Should().BeGreaterThan(14, "Expected at least 15 learners");
    }

    [Then(@"the response should contain (.*) records for page (.*)")]
    public async Task ThenTheResponseShouldContainRecordsForPage(int numberOfRecords, int pageNumber)
    {
        var testData = _context.Get<TestData>();
        var responseContent = await testData.Fm36HttpResponseMessage!.Content.ReadAsStringAsync();
        var pagedLearners = JsonConvert.DeserializeObject<PagedQueryResult<FM36Learner>>(responseContent);

        Assert.IsNotNull(pagedLearners, "Paged FM36 learners response is null");
        pagedLearners!.Items.Count.Should().Be(numberOfRecords, $"Expected {numberOfRecords} learners on page {pageNumber}");
        pagedLearners.Page.Should().Be(pageNumber, $"Expected page number to be {pageNumber}");
        pagedLearners.TotalItems.Should().BeGreaterThan(14, "Expected total items to be greater than 15");
        pagedLearners.TotalPages.Should().BeGreaterThan(2, "Expected total pages to be greater than 2");

        var linksHeader = testData.Fm36HttpResponseMessage.Headers.SingleOrDefault(x=>x.Key == "links");
        linksHeader.Should().NotBeNull("Links header is missing in the response");
    }

    private int CalculateAgeAtStart (DateTime startDate, DateTime dateOfBirth)
    {
        int age = startDate.Year - dateOfBirth.Year;

        if (startDate < dateOfBirth.AddYears(age))
        {
            age--;
        }

        return age;
    }
}