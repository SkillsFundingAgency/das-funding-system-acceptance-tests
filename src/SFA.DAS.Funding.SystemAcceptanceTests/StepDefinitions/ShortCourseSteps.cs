using NUnit.Framework.Interfaces;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Builders;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.Hooks;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions;

[Binding]
public class ShortCourseSteps(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterApiHelper, EarningsSqlClient earningsSqlClient, LearningSqlClient learningSqlClient)
{
    [Given(@"SLD informs us of a new learner with a short course starting on (.*)")]
    public async Task GivenANewLearnerWithAShortCourse(TokenisableDateTime startDate)
    {
        context.Set(new TestData(TestUlnProvider.GetNext()));
        await WhenTheProviderAddsAShortCourseForTheLearnerInTheCurrentAcademicYear(startDate);
    }

    [When(@"SLD informs us of a short course for the learner starting on (.*)")]
    public async Task WhenTheProviderAddsAShortCourseForTheLearnerInTheCurrentAcademicYear(TokenisableDateTime startDate)
    {
        var testData = context.Get<TestData>();

        var endDate = startDate.Value.AddMonths(3);

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(startDate.Value)
            .WithEndDate(endDate)
            .Build();

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);

        testData.ShortCourseLearnerData = shortCourseRequest;
    }

    [When(@"SLD informs us of a change to the short course dates pre approval")]
    public async Task WhenTheProviderChangesShortCourseDetailsPreApproval()
    {
        var testData = context.Get<TestData>();

        var newStartDate = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single().StartDate.AddMonths(2);
        var newEndDate = newStartDate.AddMonths(4);

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(newStartDate)
            .WithEndDate(newEndDate)
            .Build();

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);

        testData.ShortCourseLearnerData = shortCourseRequest;
    }

    [When(@"SLD informs us of a short course for the learner starting on (.*) with updated learner details")]
    public async Task WhenTheProviderAddsAShortCourseForTheLearnerInTheCurrentAcademicYearWithUpdatedLearnerDetails(TokenisableDateTime startDate, Table table)
    {
        var testData = context.Get<TestData>();
        
        var row = table.Rows[0];
        var firstName = row["FirstName"];
        var lastName = row["LastName"];
        var emailAddress = row["EmailAddress"];
        var dateOfBirth = DateTime.Parse(row["DateOfBirth"]);

        var endDate = startDate.Value.AddMonths(3);

        var shortCourseRequest = new ShortCourseLearnerDataBuilder(testData)
            .WithStartDate(startDate.Value)
            .WithEndDate(endDate)
            .WithLearnerDetails(firstName, lastName, emailAddress)
            .WithDateOfBirth(dateOfBirth)
            .Build();

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);

        testData.ShortCourseLearnerData = shortCourseRequest;
    }

    [When(@"the short course is approved")]
    public async Task WhenTheShortCourseIsApproved()
    {
        var testData = context.Get<TestData>();
        var shortCourseOnProgramme = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();

        var apprenticeshipCreatedEvent = new SFA.DAS.CommitmentsV2.Messages.Events.ApprenticeshipCreatedEvent
        {
            ApprenticeshipId = 123456,
            TrainingCode = shortCourseOnProgramme.CourseCode,
            ActualStartDate = shortCourseOnProgramme.StartDate,
            StartDate = shortCourseOnProgramme.StartDate,
            EndDate = shortCourseOnProgramme.ExpectedEndDate,
            PriceEpisodes = new[]
            {
                new SFA.DAS.CommitmentsV2.Messages.Events.PriceEpisode
                {
                    FromDate = shortCourseOnProgramme.StartDate,
                    Cost = 2000,
                    TrainingPrice = 2000,
                    EndPointAssessmentPrice = 0
                }
            },
            AccountId = 112,
            Uln = testData.Uln,
            FirstName = "Short",
            LastName = "CourseLearner",
            DateOfBirth = new DateTime(2000, 1, 1),
            ProviderId = Constants.UkPrn,
            LegalEntityName = "Test Legal Entity",
            IsOnFlexiPaymentPilot = true,
            LearningType = SFA.DAS.CommitmentsV2.Messages.Events.LearningType.ApprenticeshipUnit,
            TrainingCourseVersion = "1.0",
            ApprenticeshipHashedId = "ABC123",
            AccountLegalEntityId = 12345,
            TransferSenderId = null
        };

        await TestServiceBus.Das.SendApprenticeshipApprovedMessage(apprenticeshipCreatedEvent);
    }

    [When(@"SLD informs us a short course learning has completed on (.*)")]
    public async Task WhenSLDInformsUsAShortCourseLearningHasCompletedOn(TokenisableDateTime completionDate)
    {
        var testData = context.Get<TestData>();
        
        var shortCourseRequest = testData.ShortCourseLearnerData;
        shortCourseRequest.Delivery.OnProgramme.Single().CompletionDate = completionDate.Value;

        await learnerDataOuterApiHelper.AddShortCourseLearnerData(Constants.UkPrn, shortCourseRequest);
    }

    [Then(@"the basic short course earnings are generated")]
    public async Task ThenTheShortCourseIsSuccessfullyProcessed()
    {
        var testData = context.Get<TestData>();

        var expectedCourse = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();
        var expectedStartDate = expectedCourse.StartDate;
        var expectedEndDate = expectedCourse.ExpectedEndDate;

        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;
        }, "Failed to find short course earnings entity.");

        var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;

        var duration = (expectedEndDate - expectedStartDate).Days + 1;
        var daysToFirstPayment = (int)Math.Floor(duration * 0.3);
        var firstPaymentDate = expectedStartDate.AddDays(daysToFirstPayment);
        var secondPaymentDate = expectedEndDate;

        var expectedFirstPeriod = TableExtensions.Period[firstPaymentDate.ToString("MMMM")];
        var expectedFirstAcademicYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", firstPaymentDate));

        var expectedSecondPeriod = TableExtensions.Period[secondPaymentDate.ToString("MMMM")];
        var expectedSecondAcademicYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", secondPaymentDate));

        var firstInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedFirstPeriod && x.AcademicYear == expectedFirstAcademicYear);
        Assert.IsNotNull(firstInstalment, $"Could not find first instalment in period {expectedFirstPeriod} of AY {expectedFirstAcademicYear}");

        var secondInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedSecondPeriod && x.AcademicYear == expectedSecondAcademicYear);
        Assert.IsNotNull(secondInstalment, $"Could not find second instalment in period {expectedSecondPeriod} of AY {expectedSecondAcademicYear}");

        var totalPrice = earningsModel.Episodes.Single().CoursePrice;

        var expectedFirstAmount = Math.Round(totalPrice * 0.3m, 5);
        var expectedSecondAmount = Math.Round(totalPrice * 0.7m, 5);

        Assert.AreEqual((double)expectedFirstAmount, (double)firstInstalment.Amount, 0.01, "First instalment amount does not match exactly 30% of total price.");
        Assert.AreEqual((double)expectedSecondAmount, (double)secondInstalment.Amount, 0.01, "Second instalment amount does not match exactly 70% of total price.");
    }

    [Then(@"the learning domain is updated correctly")]
    public async Task ThenTheLearningDbIsUpdatedWithTheShortCourse()
    {
        var testData = context.Get<TestData>();
        var expectedLearner = testData.ShortCourseLearnerData!.Learner;
        var expectedCourse = testData.ShortCourseLearnerData.Delivery.OnProgramme.Single();

        ShortCourseLearning? learningModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            learningModel = learningSqlClient.GetShortCourseLearning(testData.Uln.ToString());
            return learningModel != null && learningModel.Episodes != null && learningModel.Episodes.Any();
        }, "Failed to find short course learning entity.");

        context.Set(learningModel);

        var learner = learningModel!.Learner;
        Assert.AreEqual(expectedLearner.FirstName, learner.FirstName, "Learner FirstName does not match.");
        Assert.AreEqual(expectedLearner.LastName, learner.LastName, "Learner LastName does not match.");
        Assert.AreEqual(expectedLearner.Email, learner.EmailAddress, "Learner EmailAddress does not match.");
        Assert.AreEqual(expectedLearner.Dob, learner.DateOfBirth, "Learner DateOfBirth does not match.");

        var episode = learningModel.Episodes.Single();
        Assert.AreEqual(expectedCourse.CourseCode, episode.TrainingCode, "TrainingCode does not match.");
        Assert.AreEqual(0, episode.EmployerAccountId, "EmployerId does not match.");
        Assert.AreEqual(Constants.UkPrn, episode.Ukprn, "Ukprn does not match.");
        Assert.AreEqual(expectedCourse.StartDate, episode.StartDate, "StartDate does not match.");
        Assert.AreEqual(expectedCourse.ExpectedEndDate, episode.ExpectedEndDate, "ExpectedEndDate does not match.");

        var expectedLearningSupports = expectedCourse.LearningSupport ?? new();
        var actualLearningSupports = episode.LearningSupport ?? new();
        Assert.AreEqual(expectedLearningSupports.Count, actualLearningSupports.Count, "LearningSupport count does not match.");
        
        foreach (var expectedSupport in expectedLearningSupports)
        {
            var actualSupport = actualLearningSupports.FirstOrDefault(x => x.StartDate == expectedSupport.StartDate && x.EndDate == expectedSupport.EndDate);
            Assert.IsNotNull(actualSupport, $"LearningSupport with StartDate {expectedSupport.StartDate} and EndDate {expectedSupport.EndDate} not found.");
        }
    }

    [Then(@"the short course is set to unapproved")]
    public void ThenTheShortCourseIsSetToUnapproved()
    {
        var learningModel = context.Get<ShortCourseLearning>();
        Assert.IsFalse(learningModel.Episodes.Single().IsApproved, "Short course should not be approved.");
    }

    [Then(@"the second instalment is earnt in period (.*)")]
    public async Task ThenTheSecondInstalmentIsEarntInPeriod(TokenisablePeriod period)
    {
        var testData = context.Get<TestData>();

        ShortCourseEarningsModel? earningsModel = null;
        await WaitHelper.WaitForIt(() =>
        {
            earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln.ToString());
            return earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;
        }, "Failed to find short course earnings entity.");

        var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;

        var expectedSecondPeriod = period.Value.PeriodValue;
        var expectedSecondAcademicYear = period.Value.AcademicYear;

        var secondInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedSecondPeriod && x.AcademicYear == expectedSecondAcademicYear);
        Assert.IsNotNull(secondInstalment, $"Could not find second instalment in period {expectedSecondPeriod} of AY {expectedSecondAcademicYear}");

        var totalPrice = earningsModel.Episodes.Single().CoursePrice;
        var expectedSecondAmount = Math.Round(totalPrice * 0.7m, 5);

        Assert.AreEqual((double)expectedSecondAmount, (double)secondInstalment.Amount, 0.01, "Second instalment amount (70% of total price) not found in expected delivery period.");
    }
}
