using Newtonsoft.Json;
using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public class PaymentsEntitySqlClient
    {
        private readonly SqlServerClient _sqlServerClient;

        public PaymentsApprenticeshipModel? GetPaymentsEntityModel(ScenarioContext context)
        {
            var earningsEvent = context.Get<EarningsGeneratedEvent>();
            var apprenticeshipKey = earningsEvent.ApprenticeshipKey;

            var apprenticeship = _sqlServerClient.GetList<PaymentsApprenticeshipModel>($"SELECT * FROM [Domain].[Apprenticeship] Where [ApprenticeshipKey] ='{apprenticeshipKey}'").Single();

            if(apprenticeship != null)
            {
                apprenticeship.Earnings = _sqlServerClient.GetList<Earnings>($"SELECT * FROM [Domain].[Earning] Where ApprenticeshipKey ='{apprenticeshipKey}'");
                apprenticeship.Payments = _sqlServerClient.GetList<TestSupport.Payments>($"SELECT * FROM [Domain].[Payment] Where ApprenticeshipKey ='{apprenticeshipKey}'");
            }

            return apprenticeship;
        }

        /// <summary>
        /// When this is instatiated it will use an existing sql server client if one exists for the connection string,
        /// otherwise it will create a new one
        /// </summary>
        public PaymentsEntitySqlClient()
        {
            var connectionString = Configurator.GetConfiguration().PaymentsDbConnectionString;
            _sqlServerClient = SqlServerClientProvider.GetSqlServerClient(connectionString);
        }
    }
}
