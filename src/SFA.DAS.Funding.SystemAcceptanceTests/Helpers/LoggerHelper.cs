namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers;

public static class LoggerHelper
{
    public static void WriteLog(string message) => TestContext.Progress.WriteLine(message);
}