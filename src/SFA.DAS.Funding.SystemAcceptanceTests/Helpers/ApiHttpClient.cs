
namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public abstract class ApiHttpClient
    { 
        protected abstract string ApiBaseUrl { get; }
        protected abstract string ApiName { get; }

        public async Task<HttpResponseMessage> Execute()
        {
            var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(ApiBaseUrl);

            return await httpClient.GetAsync(ApiName);
        }
    }
}
