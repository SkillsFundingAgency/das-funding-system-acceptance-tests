using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System.Net;
using static RestAssuredNet.RestAssuredNet;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public abstract class ApiClient
    { 
        protected abstract string ApiBaseUrl { get; }
        protected abstract string ApiName { get; }

        public async Task<HttpResponseMessage> Execute()
        {
            var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(ApiBaseUrl);

            return await httpClient.GetAsync(ApiName);
        }

        public ApprenticeshipEntityModel GetApprenticeshipEntityModel()
        {
            ApprenticeshipEntityModel apprenticeshipEntityModel = (ApprenticeshipEntityModel)Given()
                .When()
                .Get(ApiBaseUrl + ApiName)
                .AssertThat().StatusCode(HttpStatusCode.OK)
                .As(typeof(ApprenticeshipEntityModel));

            return apprenticeshipEntityModel;
        }
    }
}
