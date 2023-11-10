using Azure.Core;
using Newtonsoft.Json;
using NServiceBus;
using NUnit.Framework.Internal;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System;
using System.Net;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using static RestAssuredNet.RestAssuredNet;
using static System.Net.WebRequestMethods;

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

        public async Task<PaymentsEntityModel> GetPaymentsEntityModel()
        {
            PaymentsEntityModel paymentsEntityModel = null;

            var url = ApiBaseUrl + endpointWithParameters;

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        paymentsEntityModel = JsonConvert.DeserializeObject<PaymentsEntityModel>(content);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return paymentsEntityModel;

          
            //paymentsEntityModel = (PaymentsEntityModel)Given()
            //.When()
            //.Get(ApiBaseUrl + endpointWithParameters)
            //.AssertThat().StatusCode(HttpStatusCode.OK)
            //.As(typeof(PaymentsEntityModel));
        }
    }
}
