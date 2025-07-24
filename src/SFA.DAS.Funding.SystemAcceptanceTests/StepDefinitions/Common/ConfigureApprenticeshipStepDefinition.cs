using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Common;

[Binding]
/// <summary>
/// This will be the first step in most of the tests. It will configure the apprenticeship which will later
/// be approved/created in the ApproveApprenticeshipStepDefinition class.
/// </summary>
public class ConfigureApprenticeshipStepDefinition
{
    private readonly ScenarioContext _context;

    public ConfigureApprenticeshipStepDefinition(ScenarioContext context)
    {
        _context = context;
    }

    [Given(@"there is an apprenticeship")]
    public void CreateBasicApprenticeship()
    {
        var testData = _context.Get<TestData>();
        var startDate = TokenisableDateTime.FromString("currentAY-08-23");
        var plannedEndDate = TokenisableDateTime.FromString("currentAYPlusTwo-08-23");
        var agreedPrice = 15000;
        var trainingCode = "2";

        testData.CommitmentsApprenticeshipCreatedEvent = _context.CreateApprenticeshipCreatedMessageWithCustomValues(startDate.Value, plannedEndDate.Value, agreedPrice, trainingCode);

    }

    [Given(@"an apprenticeship has a start date of (.*), a planned end date of (.*), an agreed price of (.*), and a training code (.*)")]
    [Given(@"a learning has a start date of (.*), a planned end date of (.*), an agreed price of (.*), and a training code (.*)")]
    public void ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(TokenisableDateTime startDate, TokenisableDateTime plannedEndDate, decimal agreedPrice, string trainingCode)
    {
        var testData = _context.Get<TestData>();
        testData.CommitmentsApprenticeshipCreatedEvent = _context.CreateApprenticeshipCreatedMessageWithCustomValues(startDate.Value, plannedEndDate.Value, agreedPrice, trainingCode);
    }

    [Given(@"a learning has a start date of (.*), a planned end date of (.*) and an agreed price of (.*)")]
    public async Task LearningHasAStartDateOfAPlannedEndDateOfAndAnAgreedPrice(TokenisableDateTime startDate, TokenisableDateTime plannedEndDate, decimal agreedPrice)
    {
        var testData = _context.Get<TestData>();
        testData.CommitmentsApprenticeshipCreatedEvent = _context.CreateApprenticeshipCreatedMessageWithCustomValues(startDate.Value, plannedEndDate.Value, agreedPrice, "1");

        var approveApprenticeshipStepDefinition =
            new ApproveApprenticeshipStepDefinition(_context, new EarningsSqlClient());

        await approveApprenticeshipStepDefinition.ApproveApprenticeshipCommitment();
    }

    [Given(@"an apprenticeship with start date over (.*) months ago and duration of (.*) months and an agreed price of (.*), and a training code (.*)")]
    public void ApprenticeshipWithStartDateOverMonthsAgoAndDurationOfMonthsAndAnAgreedPriceOfAndATrainingCode(int monthsSinceStart, int duration, decimal agreedPrice, string trainingCode)
    {
        var testData = _context.Get<TestData>();
        DateTime today = DateTime.Today;
        var startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-monthsSinceStart);
        var plannedEndDate = startDate.AddMonths(duration).AddDays(-1);

        testData.CommitmentsApprenticeshipCreatedEvent = _context.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice, trainingCode);
    }

    [Given(@"an apprenticeship has a start date in the current month with a duration of (.*) months")]
    public void GivenAnApprenticeshipHasAStartDateInTheCurrentMonthWithADurationOfMonths(int duration)
    {
        var currentDate = DateTime.Today;
        var startDate = new DateTime(currentDate.Year, currentDate.Month, 1);

        var futureDate = currentDate.AddMonths(duration - 1);
        var plannedEndDate = new DateTime(futureDate.Year, futureDate.Month, DateTime.DaysInMonth(futureDate.Year, futureDate.Month));

        ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(new TokenisableDateTime(startDate), new TokenisableDateTime(plannedEndDate), 30000, "614");
    }

    [Given(@"the apprenticeship learner has a date of birth (.*)")]
    public void AddDateOfBirthToCommitmentsApprenticeshipCreatedEvent(DateTime dob)
    {
        var testData = _context.Get<TestData>();

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithDoB(testData.CommitmentsApprenticeshipCreatedEvent, dob);
    }

    [Given("the age at the start of the apprenticeship is (.*)")]
    [Given("the age at the start of the learning is (.*)")]
    public void TheAgeAtTheStartOfTheApprenticeshipIs(int age)
    {
        var testData = _context.Get<TestData>();

        var dob = testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.Value.AddYears(-age);

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithDoB(testData.CommitmentsApprenticeshipCreatedEvent, dob);
    }

    [Given("the learner is aged (.*) at the start of the apprenticeship")]
    public void LearnerIsAgedAtTheStartOfTheApprenticeship(int age)
    {
        var testData = _context.Get<TestData>();

        var dob = testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.Value.AddYears(-(age+1)).AddMonths(1);

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithDoB(testData.CommitmentsApprenticeshipCreatedEvent, dob);
    }

    [Given(@"the apprenticeship learner's age is (below|at) (.*)")]
    public void ApprenticeshipLearnersAgeIsBelowOrAt(string condition, int age)
    {
        var testData = _context.Get<TestData>();

        var dob = testData.CommitmentsApprenticeshipCreatedEvent.ActualStartDate.Value.AddYears(-age);

        if (condition == "below")
            dob = dob.AddDays(+1);

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithDoB(testData.CommitmentsApprenticeshipCreatedEvent, dob);
    }

    [Given(@"apprenticeship employer type is (Levy|NonLevy)")]
    public void EmployerTypeIsLevyOrNonLevy(string employerType)
    {
        var testData = _context.Get<TestData>();

        var employer = employerType == "Levy" ? CommitmentsV2.Types.ApprenticeshipEmployerType.Levy : CommitmentsV2.Types.ApprenticeshipEmployerType.NonLevy;

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithEmployerType(testData.CommitmentsApprenticeshipCreatedEvent, employer);
    }
}
