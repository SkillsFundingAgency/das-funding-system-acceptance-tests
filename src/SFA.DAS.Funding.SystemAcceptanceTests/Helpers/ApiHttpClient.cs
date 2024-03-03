using Newtonsoft.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public abstract class ApiClient
    {
        protected abstract string ApiBaseUrl { get; }
        protected abstract string endpointWithParameters { get; }
        public EarningsEntityModel? GetEarningsEntityModel()
        {
            string apiUrl = ApiBaseUrl + endpointWithParameters;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(apiUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    EarningsEntityModel earningsEntityModel = JsonConvert.DeserializeObject<EarningsEntityModel>(json);
                    return earningsEntityModel;
                }
                else return null;
            }
        }

        public PaymentsEntityModel? GetPaymentsEntityModel()
        {
            string apiUrl = ApiBaseUrl + endpointWithParameters;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(apiUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    string json = response.Content.ReadAsStringAsync().Result;
                    PaymentsEntityModel paymentsEntityModel = JsonConvert.DeserializeObject<PaymentsEntityModel>(json);
                    return paymentsEntityModel;
                }
                else return null;
            }
        }
    }
}
