using Microsoft.IdentityModel.Tokens;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
internal class AfterScenario
{
    private ScenarioContext _context;
    private readonly string _outputFile;

    public AfterScenario(ScenarioContext context)
    {
        _context = context;
        _outputFile = $"TESTDATA_{DateTime.Now:HH-mm-ss-fffff}.txt";
    }

    [AfterStep(Order = 10)]
    public void AfterStep()
    {
        OutputTestDataToFile();// TODO-PARALLEL - must be adapted to run in parallel
    }

    [AfterScenario(Order = 10)]
    public void AfterScenarioCleanup()
    {
        var config = Configurator.GetConfiguration();
        if (config.ShouldCleanUpTestRecords)
        {
            PurgeCreatedRecords();
        }
    }

    private void PurgeCreatedRecords()
    {
        var testData = _context.Get<TestData>();
        if (testData.ApprenticeshipKey == Guid.Empty)
            return;

        var paymentsSqlClient = _context.ScenarioContainer.Resolve<PaymentsSqlClient>();
        paymentsSqlClient.DeletePayments(testData.ApprenticeshipKey);

        var earningsSqlClient = _context.ScenarioContainer.Resolve<EarningsSqlClient>();
        earningsSqlClient.DeleteEarnings(testData.ApprenticeshipKey);

        var apprenticeshipSqlClient = new ApprenticeshipsSqlClient();
        apprenticeshipSqlClient.DeleteApprenticeship(testData.ApprenticeshipKey);
    }

    private void OutputTestDataToFile()
    {
        string StepOutcome() => _context.TestError != null ? "ERROR" : "Done";

        var stepInfo = _context.StepContext.StepInfo;

        _context.Set($"-> {StepOutcome()}: {stepInfo.StepDefinitionType} {stepInfo.Text}");

        IDictionary<string, string> testData = new Dictionary<string, string>();

        foreach (KeyValuePair<string, object> kvp in _context)
        {
            string valueString = kvp.Value != null ? kvp.Value.ToString() : null;
            testData[kvp.Key] = valueString;
        }

        using (StreamWriter writer = File.CreateText(_outputFile))
        {
            foreach (KeyValuePair<string, string> kvp in testData)
            {
                writer.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        // Attach the test data file to the test output
        TestContext.AddTestAttachment(_outputFile);
    }
}
