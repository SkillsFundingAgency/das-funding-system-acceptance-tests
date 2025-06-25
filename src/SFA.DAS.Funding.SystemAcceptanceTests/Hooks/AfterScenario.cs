using Microsoft.IdentityModel.Tokens;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using System.Text.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
internal class AfterScenario
{
    private ScenarioContext _context;
    private const string OutputFile = "OutputFile";
    private FundingConfig _config = Configurator.GetConfiguration();

    public AfterScenario(ScenarioContext context)
    {
        _context = context;
        if (_context.ScenarioInfo.Tags.Contains("releasesPayments") && !_config.ShouldReleasePayments) return;
        
        
        var testData = _context.Get<TestData>();
        _context.Set($"TESTDATA_{testData.Uln}_{DateTime.Now:HH-mm-ss-fffff}.txt", OutputFile);
    }

    [AfterStep(Order = 10)]
    public void AfterStep()
    {
        if (_context.ScenarioInfo.Tags.Contains("releasesPayments") && !_config.ShouldReleasePayments) return;
        OutputTestDataToFile();
    }

    [AfterScenario(Order = 10)]
    public void AfterScenarioCleanup()
    {
        if (_context.ScenarioInfo.Tags.Contains("releasesPayments") && !_config.ShouldReleasePayments) return;
        if (_config.ShouldCleanUpTestRecords)
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

        var apprenticeshipSqlClient = _context.ScenarioContainer.Resolve<LearningSqlClient>();
        apprenticeshipSqlClient.DeleteApprenticeship(testData.ApprenticeshipKey);
    }

    private void OutputTestDataToFile()
    {
        var testData = _context.Get<TestData>();
        var outputFile = _context.Get<string>(OutputFile);
        var hasError = _context.TestError != null;

        string StepOutcome() => hasError ? "ERROR" : "Done";

        var stepInfo = _context.StepContext.StepInfo;

        _context.Set($"-> {StepOutcome()}: {stepInfo.StepDefinitionType} {stepInfo.Text}");

        IDictionary<string, string> testLogs = new Dictionary<string, string>();

        foreach (KeyValuePair<string, object> kvp in _context)
        {
            string valueString = kvp.Value != null ? kvp.Value.ToString() : null;
            testLogs[kvp.Key] = valueString;
        }

        using (StreamWriter writer = File.CreateText(outputFile))
        {
            writer.WriteLine($"Test: {_context.ScenarioInfo.Title}");

            writer.WriteLine($"ApprenticeshipKey: {testData.ApprenticeshipKey}");
            writer.WriteLine($"Uln: {testData.Uln}");
            if(testData.ApprenticeshipCreatedEvent != null)
            {
                writer.WriteLine($"ApprenticeshipId: {testData.ApprenticeshipCreatedEvent.ApprovalsApprenticeshipId}");
            }

            if (hasError)
            {
                writer.WriteLine("PaymentsGeneratedEvent");
                writer.WriteLine(JsonSerializer.Serialize(testData.PaymentsGeneratedEvent, new JsonSerializerOptions { WriteIndented = true }));

                writer.WriteLine("EarningsGeneratedEvent");
                writer.WriteLine(JsonSerializer.Serialize(testData.EarningsGeneratedEvent, new JsonSerializerOptions { WriteIndented = true }));
            }

            foreach (KeyValuePair<string, string> kvp in testLogs)
            {
                writer.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        // Attach the test data file to the test output
        TestContext.AddTestAttachment(outputFile);
    }
}
