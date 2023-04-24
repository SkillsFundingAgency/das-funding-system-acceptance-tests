using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System.Net;
using static RestAssuredNet.RestAssuredNet;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public abstract class ApiClient
    { 
        protected abstract string ApiBaseUrl { get; }
        protected abstract string endpointWithParameters { get; }
        public EarningsEntityModel GetEarningsEntityModel()
        {
            EarningsEntityModel earningsEntityModel = (EarningsEntityModel)Given()
                .When()
                .Get(ApiBaseUrl + endpointWithParameters)
                .AssertThat().StatusCode(HttpStatusCode.OK)
                .As(typeof(EarningsEntityModel));

            return earningsEntityModel;
        }

        public PaymentsEntityModel GetPaymentsEntityModel()
        {
            PaymentsEntityModel paymentsEntityModel = (PaymentsEntityModel)Given()
                .When()
                .Get(ApiBaseUrl + endpointWithParameters)
                .AssertThat().StatusCode(HttpStatusCode.OK)
                .As(typeof(PaymentsEntityModel));

            return paymentsEntityModel;
        }
    }
}
