public class WaitHelper
{
    public static async Task WaitForIt(Func<bool> lookForIt, string failText)
    {
        var endTime = DateTime.Now.Add(TimeSpan.FromSeconds(240));

        while (DateTime.Now <= endTime)
        {
            if (lookForIt()) return;

            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }

        Assert.Fail($"{failText}  Time: {DateTime.Now:G}.");
    }

    public static async Task WaitForUnexpected(Func<bool> findUnexpected, string failText)
    {
        var endTime = DateTime.Now.Add(TimeSpan.FromSeconds(240));

        while (DateTime.Now < endTime)
        {
            if (findUnexpected())
            {
                Assert.Fail($"{failText} Time: {DateTime.Now:G}.");
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }
    }
}
