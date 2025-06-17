using Newtonsoft.Json;
using System.Collections.Concurrent;
using TechTalk.SpecFlow.CommonModels;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;


public class PaymentsFunctionsClient
{
    private readonly HttpClient _apiClient;
    private readonly string _code;

    private static readonly ConcurrentDictionary<string, TimedReleaseGate> _gates = new();
    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(40);

    public PaymentsFunctionsClient()
    {
        var config = Configurator.GetConfiguration();
        var baseUrl = config.PaymentsFunctionsBaseUrl;
        _apiClient = HttpClientProvider.GetClient(baseUrl);
        _code = config.PaymentsFunctionKey;
    }

    public async Task InvokeReleasePaymentsHttpTrigger(ScenarioContext context, byte collectionPeriod, short collectionYear)
    {
        if (!context.ScenarioInfo.Tags.Contains("releasesPayments"))
            throw new InvalidOperationException("This step can only be used in scenarios tagged with 'releasesPayments'");
        
        var testData = context.Get<TestData>();

        var key = $"{collectionYear}-{collectionPeriod}";

        var gate = _gates.GetOrAdd(key, _ =>
            new TimedReleaseGate(
                DebounceWindow,
                () => InternalInvokeReleasePaymentsHttpTrigger(collectionPeriod, collectionYear)
            ));

        testData.ReleasePaymentsOrchestrationId = await gate.WaitAndReleaseAsync();
    }

    private async Task<string> InternalInvokeReleasePaymentsHttpTrigger(byte collectionPeriod, short collectionYear)
    {
        string instanceId = string.Empty;

        await WaitUntilPaymentReleaseHasFinishedAsync();// wait for any previous payment release to finish before starting a new one

        var path = $"/api/releasePayments/{collectionYear}/{collectionPeriod}";
        var response = await _apiClient.PostAsync($"{path}?code={_code}", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Payments Http Trigger failed. {response.StatusCode} -  {response.ReasonPhrase}\nBaseUrl: {_apiClient.BaseAddress}\nPath:{path}");
        }

        if(response.Headers.TryGetValues("InstanceId", out var InstanceIdHeader))
        {
            if(InstanceIdHeader != null && InstanceIdHeader.Any())
                instanceId = InstanceIdHeader.FirstOrDefault()!;
        }

        await WaitUntilPaymentReleaseHasFinishedAsync(collectionPeriod, collectionYear); // wait for the new payment release to finish

        return instanceId;
    }

    private async Task WaitUntilPaymentReleaseHasFinishedAsync(
        byte? collectionPeriod = null, short? collectionYear = null, int pollIntervalSeconds = 5, int maxWaitSeconds = 300)
    {
        var totalWaitTime = 0;

        while (await IsPaymentReleaseRunning(collectionPeriod, collectionYear))
        {
            if (totalWaitTime >= maxWaitSeconds)
            {
                throw new TimeoutException("Timed out waiting for ReleasePaymentsOrchestration to finish.");
            }

            await Task.Delay(TimeSpan.FromSeconds(pollIntervalSeconds));
            totalWaitTime += pollIntervalSeconds;
        }
    }

    private async Task<bool> IsPaymentReleaseRunning(byte? collectionPeriod = null, short? collectionYear = null)
    {
        var path = "/runtime/webhooks/durabletask/instances";
        var response = await _apiClient.GetAsync($"{path}?runtimeStatus=Running,Pending&code={_code}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Query to orchestration failed. {response.StatusCode} -  {response.ReasonPhrase}\nBaseUrl: {_apiClient.BaseAddress}\nPath:{path}");
        }
        
        var json = await response.Content.ReadAsStringAsync();
        var orchestrations = JsonConvert.DeserializeObject<List<OrchestrationInstance<object>>>(json);

        if(orchestrations == null)
            return false;

        if (collectionPeriod.HasValue && collectionYear.HasValue)
            orchestrations = orchestrations.Where(o => {
                if (o.Input is CollectionDetails details)
                {
                    return details.CollectionPeriod == collectionPeriod.Value && details.CollectionYear == collectionYear.Value;
                }
                return false;
            }).ToList();

        return orchestrations.Any(o => o.Name == "ReleasePaymentsOrchestration");
    }


}


public class OrchestrationInstance<T>
{
    public string Name { get; set; }
    public string InstanceId { get; set; }
    public string RuntimeStatus { get; set; }
    public T Input { get; set; }
    public object CustomStatus { get; set; }
    public object Output { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime LastUpdatedTime { get; set; }
}

public class CollectionDetails
{
    public int CollectionPeriod { get; set; }
    public int CollectionYear { get; set; }
}

internal class TimedReleaseGate
{
    private readonly TimeSpan _waitWindow;
    private readonly Func<Task<string>> _releaseAction;
    private readonly object _lock = new();

    private bool _isExecuting = false;
    private Task _executionTask = Task.FromResult(string.Empty); // although this appears not to be used, it is necessary hold a reference to the task to ensure it is not garbage collected before completion.
    private readonly List<Waiter> _waiters = new();

    public TimedReleaseGate(TimeSpan waitWindow, Func<Task<string>> releaseAction)
    {
        _waitWindow = waitWindow;
        _releaseAction = releaseAction;
    }

    public Task<string> WaitAndReleaseAsync()
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
       
        lock (_lock)
        {
            _waiters.Add(new Waiter(tcs));

            if (!_isExecuting)
            {
                _isExecuting = true;

                _executionTask = Task.Run(async () =>
                {
                    await Task.Delay(_waitWindow);

                    try
                    {
                        var result = await _releaseAction();
                        CompleteAllWaiters(result);
                    }
                    catch (Exception ex)
                    {
                        CompleteAllWaiters(null, ex);
                    }

                    lock (_lock)
                    {
                        _isExecuting = false;
                    }
                });
            }
        }

        return tcs.Task;
    }

    private void CompleteAllWaiters(string? result, Exception? error = null)
    {
        lock (_lock)
        {
            foreach (var waiter in _waiters)
            {
                if (error == null)
                {
                    waiter.TaskCompletionSource.TrySetResult(result!);
                }
                else
                {
                    waiter.TaskCompletionSource.TrySetException(error!);
                }
            }

            _waiters.Clear();
        }
    }

    internal class Waiter
    {
        internal TaskCompletionSource<string> TaskCompletionSource { get; set; }
        internal Waiter(TaskCompletionSource<string> tcs)
        {
            TaskCompletionSource = tcs;
        }
    }
}
