namespace SFA.DAS.Funding.SystemAcceptanceTests.Hooks;

[Binding]
public class ParallelizationHooks
{
    private static readonly object _lock = new object();
    private bool _lockAcquired;

    [BeforeScenario("nonparallelizable")]
    public void BeforeNonParallelScenario()
    {
        Monitor.Enter(_lock);
        _lockAcquired = true;
    }

    [AfterScenario("nonparallelizable")]
    public void AfterNonParallelScenario()
    {
        if (_lockAcquired && Monitor.IsEntered(_lock))
        {
            Monitor.Exit(_lock);
            _lockAcquired = false;
        }
    }
}