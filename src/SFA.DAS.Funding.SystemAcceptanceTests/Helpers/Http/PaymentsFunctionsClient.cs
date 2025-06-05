using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;


public class PaymentsFunctionsClient
{
    private readonly HttpClient _apiClient;
    private readonly string _code;

    private static readonly ConcurrentDictionary<string, DebouncedExecution> _executions = new();

    private static readonly TimeSpan DebounceWindow = TimeSpan.FromSeconds(3);//TODO: increase time when we start batching releases

    public PaymentsFunctionsClient()
    {
        var config = Configurator.GetConfiguration();
        var baseUrl = config.PaymentsFunctionsBaseUrl;
        _apiClient = HttpClientProvider.GetClient(baseUrl);
        _code = config.PaymentsFunctionKey;
    }

    public Task InvokeReleasePaymentsHttpTrigger(byte collectionPeriod, short collectionYear)
    {
        var key = $"{collectionYear}-{collectionPeriod}";

        var execution = _executions.GetOrAdd(key, _ => new DebouncedExecution(() => InternalInvokeReleasePaymentsHttpTrigger(collectionPeriod, collectionYear), DebounceWindow));
        return execution.AwaitExecution();
    }

    private async Task InternalInvokeReleasePaymentsHttpTrigger(byte collectionPeriod, short collectionYear)
    {
        await WaitUntilPaymentReleaseHasFinishedAsync();// wait for any previous payment release to finish before starting a new one

        var path = $"/api/releasePayments/{collectionYear}/{collectionPeriod}";
        var response = await _apiClient.PostAsync($"{path}?code={_code}", null);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Payments Http Trigger failed. {response.StatusCode} -  {response.ReasonPhrase}\nBaseUrl: {_apiClient.BaseAddress}\nPath:{path}");
        }

        await WaitUntilPaymentReleaseHasFinishedAsync(collectionPeriod, collectionYear); // wait for the new payment release to finish
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


/// <summary>
/// Provides a debounced execution mechanism for asynchronous operations.
/// 
/// This class wont execute the provided action until the specified debounce window has passed without any new calls.
/// then all calls will be executed together.
/// </summary>
internal class DebouncedExecution
{
    private readonly Func<Task> _action;
    private readonly TimeSpan _debounceWindow;
    private readonly object _lock = new();

    private CancellationTokenSource? _cts;
    private TaskCompletionSource<bool> _tcs;

    public DebouncedExecution(Func<Task> action, TimeSpan debounceWindow)
    {
        _action = action;
        _debounceWindow = debounceWindow;
        _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public Task AwaitExecution()
    {
        lock (_lock)
        {
            _cts?.Cancel(); // cancel any pending debounce timer

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // restart debounce timer
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_debounceWindow, token);
                    await _action();
                    _tcs.TrySetResult(true);
                }
                catch (OperationCanceledException)
                {
                    // debounce timer reset
                }
                catch (Exception ex)
                {
                    _tcs.TrySetException(ex);
                }
                finally
                {
                    // reset for next batch
                    lock (_lock)
                    {
                        _cts.Dispose();
                        _cts = null;
                        _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    }
                }
            });

            return _tcs.Task;
        }
    }
}