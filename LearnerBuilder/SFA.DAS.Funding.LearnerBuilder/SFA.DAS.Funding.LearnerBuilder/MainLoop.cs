using AutoFixture;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.MessageBus;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System.Linq.Expressions;
using System.Reflection;

namespace SFA.DAS.Funding.LearnerBuilder;

internal class MainLoop
{
    private readonly FundingConfig _config;
    private readonly TestMessageBus _messageBus;
    private readonly Fixture _fixture;
    private readonly Queue<string> _testUlns;
    private bool _isRunning;

    internal MainLoop(FundingConfig fundingConfig, TestMessageBus testMessageBus)
    {
        _config = fundingConfig;
        _messageBus = testMessageBus;
        _fixture = new Fixture();
        _testUlns = new Queue<string>(TestUlnProvider.Initialise(100));
    }

    internal async Task Run()
    {
        _isRunning = true;

        while(_isRunning)
        {
            DisplayInstructions();

            var key = Console.ReadKey();

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    await CreateDefaultLearner();
                    break;
                case ConsoleKey.Spacebar:
                    await CreateCustomLearner();
                    break;
                case ConsoleKey.Escape:
                    _isRunning = false;
                    break;
                default:
                    Console.WriteLine("Invalid key. Please try again.");
                    break;
            }
        }
    }

    private ApprenticeshipCreatedEvent GetApprenticeshipCreatedEvent()
    {
        var startDate = TokenisableDateTime.FromString("currentAY-08-23").Value;
        var plannedEndDate = TokenisableDateTime.FromString("currentAYPlusTwo-08-23").Value;
        var agreedPrice = 15000;
        var trainingCode = "274";

        var uln = _testUlns.Dequeue;
        var ukPrn = 88888888;

        return _fixture.Build<ApprenticeshipCreatedEvent>()
           .With(_ => _.StartDate, new DateTime(startDate.Year, startDate.Month, 1))
           .With(_ => _.ActualStartDate, startDate)
           .With(_ => _.EndDate, plannedEndDate)
           .With(_ => _.PriceEpisodes, new PriceEpisodeHelper().CreateSinglePriceEpisodeUsingStartDate(startDate, agreedPrice))
           .With(_ => _.Uln, uln)
           .With(_ => _.TrainingCode, trainingCode)
           .With(_ => _.ApprenticeshipEmployerTypeOnApproval, ApprenticeshipEmployerType.Levy)
           .With(_ => _.AccountId, 3871)
           .With(_ => _.TransferSenderId, (long?)null)
           .With(_ => _.DateOfBirth, DateTime.Now.AddYears((-25)))
           .With(_ => _.IsOnFlexiPaymentPilot, true)
           .With(_ => _.TrainingCourseVersion, "1.0")
           .With(_ => _.ProviderId, ukPrn)
           .With(_ => _.FirstName, "CreateBy")
           .With(_ => _.LastName, "LearnerBuilder")
           .Create();
    }

    private async Task CreateDefaultLearner()
    {
        var createLearnerEvent = GetApprenticeshipCreatedEvent();
        await _messageBus.SendApprenticeshipApprovedMessage(createLearnerEvent);
        Console.WriteLine($"Learner created with ULN:{createLearnerEvent.Uln}");
    }

    private async Task CreateCustomLearner()
    {
        var createLearnerEvent = GetApprenticeshipCreatedEvent();

        AskChangeValue(createLearnerEvent, x => x.ActualStartDate);
        AskChangeValue(createLearnerEvent, x => x.EndDate);

        await _messageBus.SendApprenticeshipApprovedMessage(createLearnerEvent);
        Console.WriteLine($"Learner created with ULN:{createLearnerEvent.Uln}");
    }

    private static void DisplayInstructions()
    {
        Console.WriteLine("");
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine("                     PRESS");
        Console.WriteLine("            Enter : To Generate default learner");
        Console.WriteLine("            Space : To Configure learner before sending");
        Console.WriteLine("           Escape : To Exit");
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine("");
    }

    public static void AskChangeValue<T, TProp>(T obj, Expression<Func<T, TProp>> selector)
    {
        if (selector.Body is not MemberExpression memberExp ||
            memberExp.Member is not PropertyInfo propInfo)
        {
            throw new ArgumentException("Selector must be a property expression, e.g. x => x.FieldName");
        }

        var currentValue = propInfo.GetValue(obj);
        var fieldName = propInfo.Name;

        Console.WriteLine($"{fieldName} is currently '{currentValue}'. Would you like to change it? (y/n)");

        var answer = Console.ReadLine();
        if (!string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase))
            return;

        Console.WriteLine($"Enter new value for {fieldName}:");
        var input = Console.ReadLine();

        object? parsedValue = null;

        if (typeof(TProp) == typeof(string))
        {
            parsedValue = input;
        }
        else if (typeof(TProp) == typeof(int) && int.TryParse(input, out var intVal))
        {
            parsedValue = intVal;
        }
        else if ((typeof(TProp) == typeof(DateTime) || typeof(TProp) == typeof(DateTime?)) && DateTime.TryParse(input, out var dtVal))
        {
            parsedValue = dtVal;
        }
        else
        {
            Console.WriteLine($"Unsupported type {typeof(TProp).Name}. No change applied.");
            return;
        }

        propInfo.SetValue(obj, parsedValue);
        Console.WriteLine($"{fieldName} updated to '{parsedValue}'.");
        Console.WriteLine($"");
    }
}
