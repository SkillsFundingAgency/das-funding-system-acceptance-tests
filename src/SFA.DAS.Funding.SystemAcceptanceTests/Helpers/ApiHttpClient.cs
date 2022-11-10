using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System.Net;
using static RestAssuredNet.RestAssuredNet;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public abstract class ApiClient
    { 
        protected abstract string ApiBaseUrl { get; }
        protected abstract string endpointWithParameters { get; }
        public ApprenticeshipEntityModel GetApprenticeshipEntityModel()
        {
            ApprenticeshipEntityModel apprenticeshipEntityModel = (ApprenticeshipEntityModel)Given()
                .When()
                .Get(ApiBaseUrl + endpointWithParameters)
                .AssertThat().StatusCode(HttpStatusCode.OK)
                .As(typeof(ApprenticeshipEntityModel));

            return apprenticeshipEntityModel;
        }
    }
}
