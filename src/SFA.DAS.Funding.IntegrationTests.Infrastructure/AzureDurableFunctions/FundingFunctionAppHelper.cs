﻿using Newtonsoft.Json;
using SFA.DAS.Funding.IntegrationTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.IntegrationTests.Infrastructure.AzureDurableFunctions
{
    public abstract class FundingFunctionAppHelper
    {
        protected OrchestratorStartResponse OrchestratorStartResponse;
        protected HttpClient HttpClient;
        protected string BaseUrl;
        protected string AuthenticationCode;

        protected FundingFunctionAppHelper(FundingConfig config)
        {
            BaseUrl = config.FunctionsBaseUrl;
            AuthenticationCode = config.FunctionsAuthenticationCode;
            HttpClient = new HttpClient();
        }

        protected async Task StartOrchestrator(string path, bool ignoreFailure = false)
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/{path}?code={AuthenticationCode}");

            if (ignoreFailure) return;
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unsuccessful request - {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            OrchestratorStartResponse = JsonConvert.DeserializeObject<OrchestratorStartResponse>(json);
        }

        protected async Task CallHttpTrigger(string path)
        {
            var response = await HttpClient.GetAsync($"{BaseUrl}/{path}?code={AuthenticationCode}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception();
            }
        }

        protected async Task WaitUntilStatus(TimeSpan? timeout, bool continueOnFailure, params string[] status)
        {
            await WaitUntil(x => status.Contains(x.RuntimeStatus), timeout, continueOnFailure);
        }

        protected async Task WaitUntilCustomStatus(string customStatus, TimeSpan? timeout)
        {
            await WaitUntil(x => x.CustomStatus == customStatus, timeout);
        }

        private async Task WaitUntil(Func<OrchestratorStatusResponse, bool> comparison, TimeSpan? timeout, bool continueOnFailure = false)
        {
            using var cts = new CancellationTokenSource();
            if (timeout != null)
            {
                cts.CancelAfter(timeout.Value);
            }

            while (!cts.Token.IsCancellationRequested)
            {
                var response = await HttpClient.GetAsync(OrchestratorStartResponse.StatusQueryGetUri);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception();
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var orchestratorStatusResponse = JsonConvert.DeserializeObject<OrchestratorStatusResponse>(json);
                if (!continueOnFailure && orchestratorStatusResponse.RuntimeStatus == "Failed")
                {
                    throw new Exception(orchestratorStatusResponse.Output);
                }
                if (comparison(orchestratorStatusResponse))
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            throw new Exception("Orchestrator didn't complete successfully");
        }
    }
}
