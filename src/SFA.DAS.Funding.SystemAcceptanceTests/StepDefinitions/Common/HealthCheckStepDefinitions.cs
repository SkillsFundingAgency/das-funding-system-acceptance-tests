using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions.Common;

[Binding]
public class HealthCheckStepDefinitions(ScenarioContext context, LearnerDataOuterApiClient learnerDataOuterClient)
{
    private const string LearnerDataHealthcheckResultKey = "LearnerDataHealthcheckResult";

    [When(@"Call Learner Data API healthcheck endpoint")]
    public async Task CallHealthcheck()
    {
        var healthCheckReturnCode = await learnerDataOuterClient.CallHealthCheck();
        context.Set(healthCheckReturnCode, LearnerDataHealthcheckResultKey);
    }

    [Then(@"Healthcheck returns 200 OK")]
    public async Task ValidateResponse()
    {
        var returnCode = context.Get<int>(LearnerDataHealthcheckResultKey);
        Assert.That(returnCode, Is.EqualTo(200), $"Healthcheck endpoint did not return 200 OK, actual return code was {returnCode}");
    }
}
