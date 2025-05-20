using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using CommitmentsMessages = SFA.DAS.CommitmentsV2.Messages.Events;

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
        var startDate = TokenisableDateTime.FromString("currentAY-08-23");
        var plannedEndDate = TokenisableDateTime.FromString("currentAYPlusTwo-08-23");
        var agreedPrice = 15000;
        var trainingCode = "2";

        var commitmentsApprenticeshipCreatedEvent = ApprenticeshipEventHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate.Value, plannedEndDate.Value, agreedPrice, trainingCode);
        _context.Set(commitmentsApprenticeshipCreatedEvent);
    }

    [Given(@"an apprenticeship has a start date of (.*), a planned end date of (.*), an agreed price of (.*), and a training code (.*)")]
    public void ApprenticeshipHasAStartDateOfAPlannedEndDateOfAnAgreedPriceOfAndACourseCourseId(TokenisableDateTime startDate, TokenisableDateTime plannedEndDate, decimal agreedPrice, string trainingCode)
    {
        var commitmentsApprenticeshipCreatedEvent = ApprenticeshipEventHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate.Value, plannedEndDate.Value, agreedPrice, trainingCode);
        _context.Set(commitmentsApprenticeshipCreatedEvent);
    }

    [Given(@"an apprenticeship with start date over (.*) months ago and duration of (.*) months and an agreed price of (.*), and a training code (.*)")]
    public void ApprenticeshipWithStartDateOverMonthsAgoAndDurationOfMonthsAndAnAgreedPriceOfAndATrainingCode(int monthsSinceStart, int duration, decimal agreedPrice, string trainingCode)
    {
        DateTime today = DateTime.Today;
        var startDate = new DateTime(today.Year, today.Month, 1).AddMonths(-monthsSinceStart);
        var plannedEndDate = startDate.AddMonths(duration).AddDays(-1);

        var commitmentsApprenticeshipCreatedEvent = ApprenticeshipEventHelper.CreateApprenticeshipCreatedMessageWithCustomValues(startDate, plannedEndDate, agreedPrice, trainingCode);
        _context.Set(commitmentsApprenticeshipCreatedEvent);
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
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithDoB(commitmentsApprenticeshipCreatedEvent, dob);
    }

    [Given("the age at the start of the apprenticeship is (.*)")]
    public void TheAgeAtTheStartOfTheApprenticeshipIs(int age)
    {
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var dob = commitmentsApprenticeshipCreatedEvent.ActualStartDate.Value.AddYears(-age);

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithDoB(commitmentsApprenticeshipCreatedEvent, dob);
    }

    [Given("the learner is aged (.*) at the start of the apprenticeship")]
    public void LearnerIsAgedAtTheStartOfTheApprenticeship(int age)
    {
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var dob = commitmentsApprenticeshipCreatedEvent.ActualStartDate.Value.AddYears(-(age+1)).AddMonths(1);

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithDoB(commitmentsApprenticeshipCreatedEvent, dob);
    }



    [Given(@"the apprenticeship learner's age is (below|at) (.*)")]
    public void ApprenticeshipLearnersAgeIsBelowOrAt(string condition, int age)
    {
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var dob = commitmentsApprenticeshipCreatedEvent.ActualStartDate.Value.AddYears(-age);

        if (condition == "below")
            dob = dob.AddDays(+1);

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithDoB(commitmentsApprenticeshipCreatedEvent, dob);
    }

    [Given(@"apprenticeship employer type is (Levy|NonLevy)")]
    public void EmployerTypeIsLevyOrNonLevy(string employerType)
    {
        var commitmentsApprenticeshipCreatedEvent = _context.Get<CommitmentsMessages.ApprenticeshipCreatedEvent>();

        var employer = employerType == "Levy" ? CommitmentsV2.Types.ApprenticeshipEmployerType.Levy : CommitmentsV2.Types.ApprenticeshipEmployerType.NonLevy;

        ApprenticeshipEventHelper.UpdateApprenticeshipCreatedMessageWithEmployerType(commitmentsApprenticeshipCreatedEvent, employer);
    }
}
