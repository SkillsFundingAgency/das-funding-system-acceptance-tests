using SFA.DAS.Funding.SystemAcceptanceTests.Infrastructure.Configuration;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public  class EarningsEntityApiClient : ApiClient
    {
        private readonly ScenarioContext _context;
        private readonly FundingConfig _config;
        private EarningsGeneratedEvent _earnings;

        public EarningsEntityApiClient(ScenarioContext context)
        {
            _context = context;
            _config = _context.Get<FundingConfig>();
            _earnings = _context.Get<EarningsGeneratedEvent>();
        }

        protected override string ApiBaseUrl => _config.EarningsEntityApi_BaseUrl;

        protected override string endpointWithParameters => $"/runtime/webhooks/durabletask/entities/ApprenticeshipEntity/{_earnings.ApprenticeshipKey}?code={_config.EarningsFunctionKey}";
    }
}
