public class WaitHelper
{
    public static async Task WaitForIt(Func<bool> lookForIt, string failText)
    {
        var endTime = DateTime.Now.Add(TimeSpan.FromSeconds(20));
        var lastRun = false;

        while (DateTime.Now < endTime || lastRun)
        {
            if (lookForIt())
            {
                if (lastRun) return;
                lastRun = true;
            }
            else
            {
                if (lastRun) break;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(10));
        }
        Assert.Fail($"{failText}  Time: {DateTime.Now:G}.");
    }
}
