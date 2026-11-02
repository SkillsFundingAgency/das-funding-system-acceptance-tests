using DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using static SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http.LearnerDataOuterApiClient;

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
    private readonly LearnerDataOuterApiHelper _learnerDataOuterApiHelper;

    public Fm36StepDefinitions(
        ScenarioContext context,
        EarningsOuterClient earningsOuterClient,
        LearnerDataOuterApiClient learnerDataOuterApiClient,
        LearningSqlClient apprenticeshipSqlClient,
        EarningsSqlClient earningsSqlClient,
        EarningsInnerApiHelper earningsInnerApiHelper,
        LearnerDataOuterApiHelper learnerDataOuterApiHelper)
    {
        _context = context;
        _earningsOuterClient = earningsOuterClient;
        _learnerDataOuterApiClient = learnerDataOuterApiClient;
        _apprenticeshipSqlClient = apprenticeshipSqlClient;
        _earningsSqlClient = earningsSqlClient;
        _earningsInnerApiHelper = earningsInnerApiHelper;
        _learnerDataOuterApiHelper = learnerDataOuterApiHelper;
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
        // To ensure that data exists in the cache for FM36 retrieval, we must also PUT a record for at least
        // 15 learners as well.
        //
        // Records are created via the draft apprenticeship POST and then approved, since the legacy event
        // creation route results in no earnings
        
        var testData = _context.Get<TestData>();
        const int recordsToCreate = 15; //always create 15 fresh records without re-using ulns (and therefore cache keys)

        var createdRecords = new List<(string Uln, TokenisableDateTime StartDate, TokenisableDateTime PlannedEndDate, ApprenticeshipCreatedEvent Event, Guid LearnerKey)>();

        for (var i = 0; i < recordsToCreate; i++)
        {
            var uln = TestIdentifierProvider.GetNextUln();

            var startDate = TokenisableDateTime.FromString("currentAY-08-23");
            var plannedEndDate = TokenisableDateTime.FromString("currentAYPlusTwo-08-23");
            testData.CommitmentsApprenticeshipCreatedEvent = _context.CreateApprenticeshipCreatedMessageWithCustomValues(startDate.Value, plannedEndDate.Value, 15000, "2");
            testData.CommitmentsApprenticeshipCreatedEvent.ApprenticeshipId = TestIdentifierProvider.GetNextApprovalsApprenticeshipId();
            testData.CommitmentsApprenticeshipCreatedEvent.Uln = uln;

            var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

            var draftLearnerData = new LearnerDataRequest
            {
                ConsumerReference = "AcceptanceTests",
                Learner = new StubLearner
                {
                    Uln = apprenticeshipCreatedEvent.Uln,
                    LearnerRef = apprenticeshipCreatedEvent.Uln,
                    Firstname = apprenticeshipCreatedEvent.FirstName,
                    Lastname = apprenticeshipCreatedEvent.LastName,
                    Dob = apprenticeshipCreatedEvent.DateOfBirth,
                    HasEhcp = false
                },
                Delivery = new StubDelivery
                {
                    OnProgramme = new[]
                    {
                        new StubOnProgramme
                        {
                            Care = new Care(),
                            StandardCode = int.Parse(apprenticeshipCreatedEvent.TrainingCode),
                            AgreementId = "1",
                            LearnAimRef = "ZPROG001",
                            PercentageOfTrainingLeft = 0,
                            IsFlexiJob = false,
                            StartDate = apprenticeshipCreatedEvent.ActualStartDate,
                            ExpectedEndDate = apprenticeshipCreatedEvent.EndDate,
                            Costs = apprenticeshipCreatedEvent.PriceEpisodes.Select(p => new CostDetails
                            {
                                TrainingPrice = (int?)p.TrainingPrice,
                                EpaoPrice = (int?)p.EndPointAssessmentPrice,
                                FromDate = p.FromDate
                            }).ToList(),
                            LearningSupport = []
                        }
                    },
                    EnglishAndMaths = []
                }
            };

            await _learnerDataOuterApiHelper.AddLearnerData(apprenticeshipCreatedEvent.ProviderId, draftLearnerData);

            var learnerKey = Guid.Empty;
            await WaitHelper.WaitForIt(() =>
            {
                var learning = _apprenticeshipSqlClient.TryGetApprenticeshipByUln(apprenticeshipCreatedEvent.Uln);
                if (learning?.Episodes == null ||
                    learning.TrainingCode.Trim() != apprenticeshipCreatedEvent.TrainingCode ||
                    !learning.Episodes.Any(e => e.Ukprn == apprenticeshipCreatedEvent.ProviderId))
                {
                    return false;
                }

                learnerKey = learning.LearnerKey;
                return true;
            }, "Failed to find draft apprenticeship in Learning DB");

            await _context.PublishApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);

            createdRecords.Add((uln, startDate, plannedEndDate, apprenticeshipCreatedEvent, learnerKey));
        }

        Thread.Sleep(5000); // Same Wait as elsewhere following approval event publish

        foreach (var record in createdRecords)
        {
            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder
                .WithCostDetails(10000, 2000, record.StartDate.Value)
                .WithStartDate(record.StartDate.Value)
                .WithExpectedEndDate(record.PlannedEndDate.Value)
                .WithStandardCode(Convert.ToInt32(record.Event.TrainingCode))
                .WithUln(long.Parse(record.Uln));

            var learnerData = learnerDataBuilder.Build();

            await _learnerDataOuterApiHelper.UpdateLearning(record.LearnerKey, learnerData);

            testData.ResetLearnerDataBuilder();
        }

        testData.CommitmentsApprenticeshipCreatedEvent = createdRecords.Last().Event;
        testData.LearnerKey = createdRecords.Last().LearnerKey;
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

    [Then(@"learner is found in the fm36 response")]
    public void LearnerInFm36Data()
    {
        var testData = _context.Get<TestData>();
        var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);
        if (fm36Learner == null)
        {
            throw new Exception($"No FM36 data found for ULN {apprenticeshipCreatedEvent.Uln}");
        }
    }


    [Then("incentives earnings are generated for learners aged 15")]
    [Then(@"fm36 data exists for that apprenticeship")]
    public void Fm36DataExists()
    {
        var testData = _context.Get<TestData>();
        var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        var apprenticeship = _apprenticeshipSqlClient.GetApprenticeship(testData.LearningKey);
        var earnings = _earningsSqlClient.GetApprenticeshipEarningsEntityModel(_context);
        if (earnings == null)
        {
            throw new Exception("Earnings data not found for the apprenticeship.");
        }

        // get your learner data 

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == apprenticeshipCreatedEvent.Uln);
        if (fm36Learner == null)
        {
            throw new Exception($"No FM36 data found for ULN {apprenticeshipCreatedEvent.Uln}");
        }

        var earningEpisode = earnings.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent);
        var learningEpisode = apprenticeship.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent.ProviderId, testData.LearningKey);

        var expectedPriceEpisodeIdentifier = "25-" + apprenticeshipCreatedEvent.TrainingCode + "-" +
                                             apprenticeshipCreatedEvent.ActualStartDate?.ToString("dd/MM/yyyy");
        var priceEpisodeInstalmentsThisPeriod = (DateTime.Today >= apprenticeshipCreatedEvent.ActualStartDate &&
                                                 DateTime.Today <= apprenticeshipCreatedEvent.EndDate)
            ? 1
            : 0;
        var totalPrice = earningEpisode.Prices.FirstOrDefault()?.AgreedPrice;
        var onProgPayment = earningEpisode.EarningsProfile.OnProgramTotal;
        var completionPayment = earningEpisode.EarningsProfile.CompletionPayment;
        var fundingBandMax = earningEpisode.FundingBandMaximum;
        var instalmentAmount = earningEpisode?.EarningsProfile.Instalments.FirstOrDefault()?.Amount;
        var currentPeriod = TableExtensions.Period[DateTime.Now.ToString("MMMM")];

        var daysInLearningThisAY = 1 + (TokenisableDateTime.FromString("currentAY-07-31").Value - apprenticeshipCreatedEvent.StartDate.Date).Days;
        var learnDelHistDaysThisApp = CalculateLearnDelHistDaysThisApp(TokenisableDateTime.FromString("currentAY-08-01").Value, apprenticeshipCreatedEvent.StartDate);

        int plannedTotalDaysInLearning = 1 + (apprenticeshipCreatedEvent.EndDate.Date - apprenticeshipCreatedEvent.StartDate.Date).Days;
        var ageAtStartOfApprenticeship = CalculateAgeAtStart(apprenticeshipCreatedEvent.StartDate, apprenticeshipCreatedEvent.DateOfBirth);
        var fundLineType = ageAtStartOfApprenticeship > 18 ? "19+ Apprenticeship (Employer on App Service)" : "16-18 Apprenticeship (Employer on App Service)";

        var firstIncentivePeriod = earningEpisode?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DeliveryPeriod;
        var secondIncentivePeriod = earningEpisode?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DeliveryPeriod;
        var firstIncentiveThresholdDate = earningEpisode?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DueDate.ToDateOnly();
        var secondIncentiveThresholdDate = earningEpisode?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DueDate.ToDateOnly();

        var firstPriceEpisode = fm36Learner.PriceEpisodes.First();
        var learningDeliveryValues = fm36Learner.LearningDeliveries.First().LearningDeliveryValues;

        var expectedActualEndDate = apprenticeshipCreatedEvent.ActualStartDate.ToDateOnly();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(testData.GetLearnerRef(), fm36Learner.LearnRefNumber,
                "Unexpected Learner Ref Number found!");
            Assert.AreEqual(2, fm36Learner.EarningsPlatform, "Unexpected Earnings Platform found!");

            //Price Episodes
            Assert.AreEqual(expectedPriceEpisodeIdentifier, firstPriceEpisode.PriceEpisodeIdentifier, "Unexpected PriceEpisodeIdentifier found!");
            Assert.AreEqual(expectedActualEndDate, firstPriceEpisode.PriceEpisodeValues.EpisodeStartDate, "Unexpected PriceEpisodeStartDate found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.PriceEpisodes.First().TrainingPrice, firstPriceEpisode.PriceEpisodeValues.TNP1, "Unexpected TNP1 value found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.PriceEpisodes.First().EndPointAssessmentPrice, firstPriceEpisode.PriceEpisodeValues.TNP2, "Unexpected TNP2 value found!");
            Assert.AreEqual(EarningsFM36Constants.TNP3, firstPriceEpisode.PriceEpisodeValues.TNP3, "Unexpected TNP3 value found!");
            Assert.AreEqual(EarningsFM36Constants.TNP4, firstPriceEpisode.PriceEpisodeValues.TNP4, "Unexpected TNP4 value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeActualEndDateIncEPA, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeActualEndDateIncEPA, "Unexpected PriceEpisodeActualEndDateIncEPA value found!");
            Assert.AreEqual(fundingBandMax, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeUpperBandLimit, "Unexpected PriceEpisodeUpperBandLimit value found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.EndDate.ToDateOnly(), firstPriceEpisode.PriceEpisodeValues.PriceEpisodePlannedEndDate, "Unexpected PriceEpisodePlannedEndDate found!");
            Assert.AreEqual(null, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeActualEndDate, "Unexpected PriceEpisodeActualEndDate found!");
            Assert.AreEqual(totalPrice, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeTotalTNPPrice, "Unexpected PriceEpisodeTotalTNPPrice value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeUpperLimitAdjustment, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeUpperLimitAdjustment, "Unexpected PriceEpisodeUpperLimitAdjustment value found!");
            Assert.AreEqual(earnings?.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)?.EarningsProfile.Instalments.Count, firstPriceEpisode.PriceEpisodeValues.PriceEpisodePlannedInstalments, "Unexpected PriceEpisodePlannedInstalments value found!");
            Assert.AreEqual(0, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeActualInstalments, "Unexpected PriceEpisodeActualInstalments value found!");
            Assert.AreEqual(priceEpisodeInstalmentsThisPeriod, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeInstalmentsThisPeriod, "Unexpected PriceEpisodeInstalmentsThisPeriod value found!");
            Assert.AreEqual(completionPayment, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeCompletionElement, "Unexpected PriceEpisodeCompletionElement value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodePreviousEarnings, firstPriceEpisode.PriceEpisodeValues.PriceEpisodePreviousEarnings, "Unexpected PriceEpisodePreviousEarnings value found!");
            Assert.AreEqual(instalmentAmount, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeInstalmentValue, "Unexpected PriceEpisodeInstalmentValue value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeOnProgPayment, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeOnProgPayment, "Unexpected PriceEpisodeOnProgPayment value found!");
            Assert.AreEqual(onProgPayment, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeTotalEarnings, "Unexpected PriceEpisodeTotalEarnings value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeBalanceValue, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeBalanceValue, "Unexpected PriceEpisodeBalanceValue value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeBalancePayment, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeBalancePayment, "Unexpected PriceEpisodeBalancePayment value found!");
            Assert.AreEqual(false, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeCompleted, "Unexpected PriceEpisodeCompleted value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeCompletionPayment, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeCompletionPayment, "Unexpected PriceEpisodeCompletionPayment value found!");
            Assert.AreEqual(fundingBandMax - (instalmentAmount * (currentPeriod - 1)), firstPriceEpisode.PriceEpisodeValues.PriceEpisodeRemainingTNPAmount, "Unexpected PriceEpisodeRemainingTNPAmount value found!");
            Assert.AreEqual(fundingBandMax - (instalmentAmount * (currentPeriod - 1)), firstPriceEpisode.PriceEpisodeValues.PriceEpisodeRemainingAmountWithinUpperLimit, "Unexpected PriceEpisodeRemainingAmountWithinUpperLimit value found!");
            Assert.AreEqual(fundingBandMax - (instalmentAmount * (currentPeriod - 1)), firstPriceEpisode.PriceEpisodeValues.PriceEpisodeCappedRemainingTNPAmount, "Unexpected PriceEpisodeCappedRemainingTNPAmount value found!");
            Assert.AreEqual(fundingBandMax - (instalmentAmount * (currentPeriod - 1)) - completionPayment, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeExpectedTotalMonthlyValue, "Unexpected PriceEpisodeExpectedTotalMonthlyValue value found!");
            Assert.AreEqual(EarningsFM36Constants.AimSeqNumber, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeAimSeqNumber, "Unexpected PriceEpisodeAimSeqNumber value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeSecondProv1618Pay, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeSecondProv1618Pay, "Unexpected PriceEpisodeSecondProv1618Pay value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeFirstEmp1618Pay, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeFirstEmp1618Pay, "Unexpected PriceEpisodeFirstEmp1618Pay value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeSecondEmp1618Pay, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeSecondEmp1618Pay, "Unexpected PriceEpisodeSecondEmp1618Pay value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeFirstProv1618Pay, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeFirstProv1618Pay, "Unexpected PriceEpisodeFirstProv1618Pay value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeLSFCash, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeLSFCash, "Unexpected PriceEpisodeLSFCash value found!");
            Assert.AreEqual(fundLineType, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeFundLineType, "Unexpected PriceEpisodeFundLineType value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeLevyNonPayInd, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeLevyNonPayInd, "Unexpected PriceEpisodeLevyNonPayInd value found!");
            Assert.AreEqual(expectedActualEndDate, firstPriceEpisode.PriceEpisodeValues.EpisodeEffectiveTNPStartDate, "Unexpected EpisodeEffectiveTNPStartDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeFirstAdditionalPaymentThresholdDate, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeFirstAdditionalPaymentThresholdDate, "Unexpected PriceEpisodeFirstAdditionalPaymentThresholdDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeSecondAdditionalPaymentThresholdDate, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeSecondAdditionalPaymentThresholdDate, "Unexpected PriceEpisodeSecondAdditionalPaymentThresholdDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeContractType, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeContractType, "Unexpected PriceEpisodeContractType value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodePreviousEarningsSameProvider, firstPriceEpisode.PriceEpisodeValues.PriceEpisodePreviousEarningsSameProvider, "Unexpected PriceEpisodePreviousEarningsSameProvider value found!");
            Assert.AreEqual(onProgPayment, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeTotProgFunding, "Unexpected PriceEpisodeTotProgFunding value found!");
            Assert.AreEqual((onProgPayment * 95) / 100, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeProgFundIndMinCoInvest, "Unexpected PriceEpisodeProgFundIndMinCoInvest value found!");
            Assert.AreEqual((onProgPayment * 5) / 100, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeProgFundIndMaxEmpCont, "Unexpected PriceEpisodeProgFundIndMaxEmpCont value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeTotalPMRs, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeTotalPMRs, "Unexpected PriceEpisodeTotalPMRs value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeCumulativePMRs, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeCumulativePMRs, "Unexpected PriceEpisodeCumulativePMRs value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeCompExemCode, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeCompExemCode, "Unexpected PriceEpisodeCompExemCode value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeLearnerAdditionalPaymentThresholdDate, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeLearnerAdditionalPaymentThresholdDate, "Unexpected PriceEpisodeLearnerAdditionalPaymentThresholdDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeRedStartDate, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeRedStartDate, "Unexpected PriceEpisodeRedStartDate value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeRedStatusCode, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeRedStatusCode, "Unexpected PriceEpisodeRedStatusCode value found!");
            Assert.AreEqual("25-" + apprenticeshipCreatedEvent.TrainingCode, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeLDAppIdent, "Unexpected PriceEpisodeLDAppIdent value found!");
            Assert.AreEqual(EarningsFM36Constants.PriceEpisodeAugmentedBandLimitFactor, firstPriceEpisode.PriceEpisodeValues.PriceEpisodeAugmentedBandLimitFactor, "Unexpected PriceEpisodeAugmentedBandLimitFactor value found!");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames
                    .PriceEpisodeApplic1618FrameworkUpliftBalancing)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeApplic1618FrameworkUpliftBalancing} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames
                    .PriceEpisodeApplic1618FrameworkUpliftCompletionPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeApplic1618FrameworkUpliftCompletionPayment} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames
                    .PriceEpisodeApplic1618FrameworkUpliftOnProgPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeApplic1618FrameworkUpliftOnProgPayment} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalancePayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalancePayment} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalanceValue)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeBalanceValue} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeCompletionPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeCompletionPayment} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstDisadvantagePayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstDisadvantagePayment} are zero.");

            if (ageAtStartOfApprenticeship < 19)
            {
                firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
                .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay} value for period {firstIncentivePeriod} is not 500");

                firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay).SingleOrDefault(x => x.PeriodNumber == firstIncentivePeriod).Value
                .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay} value for period {firstIncentivePeriod} is not 500");
            }
            else
            {
                firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstEmp1618Pay} are zero.");

                firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeFirstProv1618Pay} are zero.");
            }

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLevyNonPayInd)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLevyNonPayInd} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLSFCash)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLSFCash} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondDisadvantagePayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondDisadvantagePayment} are zero.");

            if (ageAtStartOfApprenticeship < 19)
            {
                firstPriceEpisode.PriceEpisodePeriodisedValues
               .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondEmp1618Pay).SingleOrDefault(x => x.PeriodNumber == secondIncentivePeriod).Value
               .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondEmp1618Pay} value for period {secondIncentivePeriod} is not 500");

                firstPriceEpisode.PriceEpisodePeriodisedValues
               .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondProv1618Pay).SingleOrDefault(x => x.PeriodNumber == secondIncentivePeriod).Value
               .Should().Be(500, $"{PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondProv1618Pay} value for period {secondIncentivePeriod} is not 500");
            }
            else
            {
                firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondEmp1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondEmp1618Pay} are zero.");

                firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondProv1618Pay)
                .Should().OnlyContain(x => x.Value == 0, $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeSecondProv1618Pay} are zero.");
            }

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLearnerAdditionalPayment)
                .Should().OnlyContain(x => x.Value == 0,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeLearnerAdditionalPayment} are zero.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeInstalmentsThisPeriod)
                .Should().OnlyContain(x => x.Value == 1,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeInstalmentsThisPeriod} are 1.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeOnProgPayment)
                .Should().OnlyContain(x => x.Value == instalmentAmount,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeOnProgPayment} are {instalmentAmount}.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeProgFundIndMaxEmpCont)
                .Should().OnlyContain(x => x.Value == (instalmentAmount * 5) / 100,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeProgFundIndMaxEmpCont} are {(instalmentAmount * 5) / 100}.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeProgFundIndMinCoInvest)
                .Should().OnlyContain(x => x.Value == (instalmentAmount * 95) / 100,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeProgFundIndMinCoInvest} are {(instalmentAmount * 95) / 100}.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeTotProgFunding)
                .Should().OnlyContain(x => x.Value == instalmentAmount,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeTotProgFunding} are {instalmentAmount}.");

            firstPriceEpisode.PriceEpisodePeriodisedValues
                .GetValuesForAttribute(PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeESFAContribPct)
                .Should().OnlyContain(x => x.Value == 0.95m,
                    $"Not all {PriceEpisodePeriodisedValuesAttributeNames.PriceEpisodeESFAContribPct} are 0.95.");

            // Learning Deliveries
            Assert.AreEqual(EarningsFM36Constants.AimSeqNumber, fm36Learner.LearningDeliveries.First().AimSeqNumber, "Unexpected AimSeqNumber found!");
            Assert.AreEqual(EarningsFM36Constants.ActualDaysIL, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.ActualDaysIL, "Unexpected ActualDaysIL found!");
            Assert.IsNull(learningDeliveryValues.ActualNumInstalm, "Unexpected ActualNumInstalm found!");
            Assert.AreEqual(expectedActualEndDate, learningDeliveryValues.AdjStartDate, "Unexpected AdjStartDate found!");
            Assert.AreEqual(ageAtStartOfApprenticeship, learningDeliveryValues.AgeAtProgStart, "Unexpected AgeAtProgStart found!");
            Assert.AreEqual(expectedActualEndDate, learningDeliveryValues.AppAdjLearnStartDate, "Unexpected AppAdjLearnStartDate found!");
            Assert.AreEqual(expectedActualEndDate, learningDeliveryValues.AppAdjLearnStartDateMatchPathway, "Unexpected AppAdjLearnStartDateMatchPathway found!");
            Assert.AreEqual(EarningsFM36Constants.ApplicCompDate, learningDeliveryValues.ApplicCompDate, "Unexpected ApplicCompDate found!");
            Assert.AreEqual(EarningsFM36Constants.CombinedAdjProp, learningDeliveryValues.CombinedAdjProp, "Unexpected CombinedAdjProp found!");
            Assert.AreEqual(EarningsFM36Constants.Completed, learningDeliveryValues.Completed, "Unexpected Completed found!");
            Assert.AreEqual(firstIncentiveThresholdDate, learningDeliveryValues.FirstIncentiveThresholdDate, "Unexpected FirstIncentiveThresholdDate found!");
            Assert.AreEqual(EarningsFM36Constants.FundStart, learningDeliveryValues.FundStart, "Unexpected FundStart found!");
            Assert.AreEqual(EarningsFM36Constants.LearnAimRef, learningDeliveryValues.LearnAimRef, "Unexpected LearnAimRef found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.ActualStartDate, learningDeliveryValues.LearnStartDate, "Unexpected LearnStartDate found!");
            Assert.AreEqual(ageAtStartOfApprenticeship < 19 ? true : false, learningDeliveryValues.LearnDel1618AtStart, "Unexpected LearnDel1618AtStart found!");
            Assert.IsNull(learningDeliveryValues.LearnDelAppAccDaysIL, "Unexpected LearnDelAppAccDaysIL found!");
            Assert.IsNull(learningDeliveryValues.LearnDelApplicEmp1618Incentive, "Unexpected LearnDelApplicEmp1618Incentive found!");
            Assert.IsNull(learningDeliveryValues.LearnDelApplicEmpDate, "Unexpected LearnDelApplicEmpDate found!");
            Assert.IsNull(learningDeliveryValues.LearnDelApplicProv1618Incentive, "Unexpected LearnDelApplicProv1618Incentive found!");
            Assert.IsNull(learningDeliveryValues.LearnDelAppPrevAccDaysIL, "Unexpected LearnDelAppPrevAccDaysIL found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelEmpIdFirstAdditionalPaymentThreshold, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelEmpIdFirstAdditionalPaymentThreshold, "Unexpected LearnDelEmpIdFirstAdditionalPaymentThreshold found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelEmpIdSecondAdditionalPaymentThreshold, fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelEmpIdSecondAdditionalPaymentThreshold, "Unexpected LearnDelEmpIdSecondAdditionalPaymentThreshold found!");
            Assert.IsNull(learningDeliveryValues.LearnDelHistDaysThisApp, "Unexpected LearnDelHistDaysThisApp found!");
            Assert.IsNull(learningDeliveryValues.LearnDelHistProgEarnings, "Unexpected LearnDelHistProgEarnings found!");
            Assert.AreEqual(fundLineType, learningDeliveryValues.LearnDelInitialFundLineType, "Unexpected LearnDelInitialFundLineType found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelProgEarliestACT2Date, learningDeliveryValues.LearnDelProgEarliestACT2Date, "Unexpected LearnDelProgEarliestACT2Date found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelNonLevyProcured, learningDeliveryValues.LearnDelNonLevyProcured, "Unexpected LearnDelNonLevyProcured found!");
            Assert.AreEqual(EarningsFM36Constants.MathEngAimValue, learningDeliveryValues.MathEngAimValue, "Unexpected MathEngAimValue found!");
            Assert.AreEqual(EarningsFM36Constants.OutstandNumOnProgInstalm, learningDeliveryValues.OutstandNumOnProgInstalm, "Unexpected OutstandNumOnProgInstalm found!");
            
            Assert.AreEqual(learningDeliveryValues.LearnStartDate.GetNumberOfIncludedCensusDatesUntil(apprenticeshipCreatedEvent.EndDate), learningDeliveryValues.PlannedNumOnProgInstalm, "Unexpected PlannedNumOnProgInstalm found!");
            
            Assert.IsNull(learningDeliveryValues.PlannedTotalDaysIL, "Unexpected PlannedTotalDaysIL found!");
            Assert.AreEqual(EarningsFM36Constants.ProgType, learningDeliveryValues.ProgType, "Unexpected ProgType found!");
            Assert.AreEqual(secondIncentiveThresholdDate, learningDeliveryValues.SecondIncentiveThresholdDate, "Unexpected SecondIncentiveThresholdDate found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.TrainingCode, learningDeliveryValues.StdCode.ToString(), "Unexpected StdCode found!");
            Assert.AreEqual(EarningsFM36Constants.ThresholdDays, learningDeliveryValues.ThresholdDays, "Unexpected ThresholdDays found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelApplicCareLeaverIncentive, learningDeliveryValues.LearnDelApplicCareLeaverIncentive, "Unexpected LearnDelApplicCareLeaverIncentive found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelHistDaysCareLeavers, learningDeliveryValues.LearnDelHistDaysCareLeavers, "Unexpected LearnDelHistDaysCareLeavers found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelAccDaysILCareLeavers, learningDeliveryValues.LearnDelAccDaysILCareLeavers, "Unexpected LearnDelAccDaysILCareLeavers found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelPrevAccDaysILCareLeavers, learningDeliveryValues.LearnDelPrevAccDaysILCareLeavers, "Unexpected LearnDelPrevAccDaysILCareLeavers found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelLearnerAddPayThresholdDate, learningDeliveryValues.LearnDelLearnerAddPayThresholdDate, "Unexpected LearnDelLearnerAddPayThresholdDate found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelRedCode, learningDeliveryValues.LearnDelRedCode, "Unexpected LearnDelRedCode found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelRedStartDate, learningDeliveryValues.LearnDelRedStartDate, "Unexpected LearnDelRedStartDate found!");

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

    [Then(@"fm36 contains (.*) Learning Deliveries")]
    public void ThenFM36ContainsLearningDeliveries(int expectedValue)
    {
        var testData = _context.Get<TestData>();
        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        Assert.AreEqual(expectedValue, fm36Learner!.LearningDeliveries.Count, "Unexpected number of Learning Deliveries found!");
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

        Assert.AreEqual(lastDayOfLearning.DateOnly,
            fm36Learner!.PriceEpisodes.First().PriceEpisodeValues.PriceEpisodeActualEndDate, "Unexpected PriceEpisodeActualEndDate found!");
    }

    [Then(@"fm36 block contains a new price episode starting (.*) with episode 1 tnp of (.*) and episode 2 tnp of (.*)")]
    public void ThenFm36BlockContainsANewPriceEpisodeStarting(TokenisableDateTime newEpisodeStartDate, decimal expectedEpisode1Tnp, decimal expectedEpisode2Tnp)
    {
        var testData = _context.Get<TestData>();

        // Retrieve necessary data from the context
        var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        // Fetch earnings data
        var earnings = _earningsSqlClient.GetApprenticeshipEarningsEntityModel(_context);

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
        Assert.AreEqual(newEpisodeStartDate.DateOnly, episode2.PriceEpisodeValues.EpisodeStartDate,
            $"Episode 2 start date mismatch. Expected: {newEpisodeStartDate.DateOnly}, Actual: {episode2.PriceEpisodeValues.EpisodeStartDate}");

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
        var earnings = _earningsSqlClient.GetApprenticeshipEarningsEntityModel(_context);

        // get your learner data 

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);
        var earningsEpisode = earnings?.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent);

        var firstIncentivePeriod = earningsEpisode?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DeliveryPeriod;
        var firstIncentiveThresholdDate = earningsEpisode?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MinBy(x => x.DueDate)?.DueDate.ToDateOnly();
        var secondIncentivePeriod = earningsEpisode?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DeliveryPeriod;
        var secondIncentiveThresholdDate = earningsEpisode?.AdditionalPayments.Where(x => x.AdditionalPaymentType == AdditionalPaymentType.EmployerIncentive)?.MaxBy(x => x.DueDate)?.DueDate.ToDateOnly();

        var expectedSecondIncentive = secondIncentivePeriod != firstIncentivePeriod;

        fm36Learner!.LearningDeliveries.First().LearningDeliveryValues.LearnDel1618AtStart.Should().BeTrue();
        fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicProv1618Incentive.Should().Be(null);
        fm36Learner.LearningDeliveries.First().LearningDeliveryValues.LearnDelApplicEmp1618Incentive.Should().Be(null);

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
            .Should().Be(completionDate.DateOnly, $"{EarningsFM36Constants.PriceEpisodeActualEndDateIncEPA} value is not {completionDate.DateOnly}");
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
        pagedLearners!.Items.Count.Should().BeGreaterThan(0).And.BeLessOrEqualTo(numberOfRecords, $"Expected between 1 and {numberOfRecords} learners on page {pageNumber}");
        pagedLearners.Page.Should().Be(pageNumber, $"Expected page number to be {pageNumber}");
        pagedLearners.TotalItems.Should().BeGreaterThan(14, "Expected total items to be greater than 14");
        pagedLearners.TotalPages.Should().BeGreaterThan(2, "Expected total pages to be greater than 2");

        var linksHeader = testData.Fm36HttpResponseMessage.Headers.SingleOrDefault(x => x.Key == "links");
        linksHeader.Should().NotBeNull("Links header is missing in the response");
    }

    [Then("English and maths learning deliveries section is populated correctly for learnAimRef (.*) from (.*) to (.*)")]
    public void EnglishAndMathsLearningDeliveriesSectionIsPopulatedCorrectly(string learnAimRef, byte startPeriod, byte endPeriod)
    {
        var testData = _context.Get<TestData>();

        var apprenticeshipCreatedEvent = testData.CommitmentsApprenticeshipCreatedEvent;

        var age = CalculateAgeAtStart(apprenticeshipCreatedEvent.StartDate, apprenticeshipCreatedEvent.DateOfBirth);

        var fm36Learner = testData.FM36Learners.Find(x => x.ULN.ToString() == testData.CommitmentsApprenticeshipCreatedEvent.Uln);

        var EnglishAndMathsLearnerData = testData.UpdateLearnerData?.Delivery.EnglishAndMaths.Where(x => x.LearnAimRef == learnAimRef).SingleOrDefault();

        var englishAndMathLearningDeliveries = fm36Learner!.LearningDeliveries.Where(x => x.LearningDeliveryValues.LearnAimRef == learnAimRef).SingleOrDefault();

        string fundingLineType = age < 19 ? "16-18 Apprenticeship (Employer on App Service)" : "19+ Apprenticeship (Employer on App Service)";

        var earnings = _earningsSqlClient.GetApprenticeshipEarningsEntityModel(_context);


        var englishAndMathsAmount = earnings.Episodes.GetEpisode(testData.CommitmentsApprenticeshipCreatedEvent)
            .MathsAndEnglish.Where(x => x.LearnAimRef == EnglishAndMathsLearnerData.LearnAimRef).First().Amount;

        Assert.Multiple(() =>
        {
            var learningDeliveryValues = englishAndMathLearningDeliveries?.LearningDeliveryValues;

            // Learning Deliveries
            Assert.AreEqual(age, learningDeliveryValues.AgeAtProgStart, "Unexpected AgeAtProgStart found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.ActualStartDate.ToDateOnly(), learningDeliveryValues.AppAdjLearnStartDate, "Unexpected AppAdjLearnStartDate found!");
            Assert.AreEqual(EarningsFM36Constants.FundStart, learningDeliveryValues.FundStart, "Unexpected FundStart found!");
            Assert.AreEqual(EnglishAndMathsLearnerData?.StartDate, learningDeliveryValues.LearnStartDate, "Unexpected LearnStartDate found!");
            Assert.AreEqual(age <19 ? true : false, learningDeliveryValues.LearnDel1618AtStart, "Unexpected LearnDel1618AtStart found!");
            Assert.AreEqual(fundingLineType, learningDeliveryValues.LearnDelInitialFundLineType, "Unexpected LearnDelInitialFundLinetype");
            Assert.IsTrue(learningDeliveryValues.LearnDelMathEng, "Unexpected LearnDelMathEng found!");
            Assert.AreEqual(25, learningDeliveryValues.ProgType, "Unexpected ProgType found!");
            Assert.AreEqual(apprenticeshipCreatedEvent.TrainingCode, learningDeliveryValues.StdCode.ToString(), "Unexpected StdCode found!");
            Assert.AreEqual(0, learningDeliveryValues.LearnDelRedCode, "Unexpected LearnDelRedCode found!");
            Assert.AreEqual(EarningsFM36Constants.LearnDelRedStartDate, learningDeliveryValues.LearnDelRedStartDate, "Unexpected LearnDelRedStartDate found!");
            Assert.IsNull(learningDeliveryValues.LearnDelAppPrevAccDaysIL, "Unexpected LearnDelAppPrevAccDaysIL found!");
            Assert.IsNull(learningDeliveryValues.LearnDelAppAccDaysIL, "Unexpected LearnDelAppAccDaysIL found!");
            Assert.IsNull(learningDeliveryValues.LearnDelApplicEmp1618Incentive, "Unexpected LearnDelApplicEmp1618Incentive found!");
            Assert.IsNull(learningDeliveryValues.LearnDelApplicProv1618Incentive, "Unexpected LearnDelApplicProv1618Incentive found!");
            Assert.IsNull(learningDeliveryValues.LearnDelHistDaysThisApp, "Unexpected LearnDelHistDaysThisApp found!");
            Assert.AreEqual(learningDeliveryValues.LearnStartDate.GetNumberOfIncludedCensusDatesUntil(EnglishAndMathsLearnerData.EndDate), englishAndMathLearningDeliveries?.LearningDeliveryValues.PlannedNumOnProgInstalm, "Unexpected PlannedNumOnProgInstalm found!");
            Assert.IsNull(learningDeliveryValues.PlannedTotalDaysIL, "Unexpected PlannedTotalDaysIL found!");
            Assert.IsNull(learningDeliveryValues.LearnDelHistProgEarnings, "Unexpected LearnDelHistProgEarnings found!");

             // Learning Delivery Periodised Values
            englishAndMathLearningDeliveries.LearningDeliveryPeriodisedValues
           .AssertValuesForPeriods(LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFund, startPeriod, endPeriod, 1, 0);

            englishAndMathLearningDeliveries.LearningDeliveryPeriodisedValues
              .AssertValuesForPeriods(LearningDeliveryPeriodisedValuesAttributeNames.LearnSuppFundCash, startPeriod, endPeriod, 150, 0);

            englishAndMathLearningDeliveries.LearningDeliveryPeriodisedValues
              .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.LearnDelLevyNonPayInd)
              .Should().OnlyContain(x => x.Value == 0,
                  $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.LearnDelLevyNonPayInd} are 0.");

            englishAndMathLearningDeliveries.LearningDeliveryPeriodisedValues
             .GetValuesForAttribute(LearningDeliveryPeriodisedValuesAttributeNames.MathEngBalPayment)
             .Should().OnlyContain(x => x.Value == 0,
                 $"Not all {LearningDeliveryPeriodisedValuesAttributeNames.MathEngBalPayment} are 0.");

            englishAndMathLearningDeliveries.LearningDeliveryPeriodisedValues
              .AssertValuesForPeriods(LearningDeliveryPeriodisedValuesAttributeNames.MathEngOnProgPayment, startPeriod, endPeriod, englishAndMathsAmount / endPeriod, 0);

            // Learning Delivery Periodised Text Values

            englishAndMathLearningDeliveries.LearningDeliveryPeriodisedTextValues
             .AssertValuesForPeriods(LearningDeliveryPeriodisedTextValuesAttributeNames.FundLineType, startPeriod, endPeriod, fundingLineType, "None");

            englishAndMathLearningDeliveries.LearningDeliveryPeriodisedTextValues
             .AssertValuesForPeriods(LearningDeliveryPeriodisedTextValuesAttributeNames.LearnDelContType, startPeriod, endPeriod, "Contract for services with the employer", "None");
        });
    }


    private int CalculateAgeAtStart(DateTime startDate, DateTime dateOfBirth)
    {
        int age = startDate.Year - dateOfBirth.Year;

        if (startDate < dateOfBirth.AddYears(age))
        {
            age--;
        }

        return age;
    }

    private int CalculateLearnDelHistDaysThisApp(DateTime academicYearStartDate, DateTime learningStartDate)
    {
        var start = learningStartDate;
        var end = academicYearStartDate.AddDays(-1);

        if (start > end)
            return 0;

        return (end - start).Days + 1; // inclusive
    }
}