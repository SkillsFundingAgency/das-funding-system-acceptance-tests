namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
public class ParallelizationHooks
{
    private static readonly object _lock = new object();

    [BeforeScenario("nonparallelizable")]
    public void BeforeNonParallelScenario()
    {
        Monitor.Enter(_lock);
    }

    [AfterScenario("nonparallelizable")]
    public void AfterNonParallelScenario()
    {
        if (Monitor.IsEntered(_lock))
        {
            Monitor.Exit(_lock);
        }
    }
}