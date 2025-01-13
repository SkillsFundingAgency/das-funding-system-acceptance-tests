using SFA.DAS.Funding.SystemAcceptanceTests;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

public static class WaitHelper
{
    private static FundingConfig config;
    private static int waitTime;

    static WaitHelper()
    {
        config = Configurator.GetConfiguration();
        waitTime = config.EventWaitTimeInSeconds;
    }

    public static async Task WaitForItAsync(Func<Task<bool>> lookForIt, string failText)
    {
        var endTime = DateTime.Now.Add(TimeSpan.FromSeconds(waitTime));

        while (DateTime.Now <= endTime)
        {
            if (await lookForIt()) return;

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        Assert.Fail($"{failText}  Time: {DateTime.Now:G}.");
    }

    public static async Task WaitForIt(Func<bool> lookForIt, string failText)
    {
        var endTime = DateTime.Now.Add(TimeSpan.FromSeconds(waitTime));

        while (DateTime.Now <= endTime)
        {
            if (lookForIt()) return;

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        Assert.Fail($"{failText}  Time: {DateTime.Now:G}.");
    }

    public static async Task WaitForUnexpected(Func<bool> findUnexpected, string failText)
    {
        var endTime = DateTime.Now.Add(TimeSpan.FromSeconds(waitTime));

        while (DateTime.Now < endTime)
        {
            if (findUnexpected())
            {
                Assert.Fail($"{failText} Time: {DateTime.Now:G}.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
}
