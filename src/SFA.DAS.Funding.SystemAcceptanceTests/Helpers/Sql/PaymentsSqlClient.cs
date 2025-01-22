﻿using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

public class PaymentsSqlClient
{
    private readonly SqlServerClient _sqlServerClient;

    public PaymentsApprenticeshipModel? GetPaymentsModel(ScenarioContext context)
    {
        var earningsEvent = context.Get<EarningsGeneratedEvent>();
        var apprenticeshipKey = earningsEvent.ApprenticeshipKey;

        var apprenticeship = _sqlServerClient.GetList<PaymentsApprenticeshipModel>($"SELECT * FROM [Domain].[Apprenticeship] Where [ApprenticeshipKey] ='{apprenticeshipKey}'").Single();

        if (apprenticeship != null)
        {
            apprenticeship.Earnings = _sqlServerClient.GetList<Earnings>($"SELECT * FROM [Domain].[Earning] Where ApprenticeshipKey ='{apprenticeshipKey}'");
            apprenticeship.Payments = _sqlServerClient.GetList<TestSupport.Payments>($"SELECT * FROM [Domain].[Payment] Where ApprenticeshipKey ='{apprenticeshipKey}'");
        }

        return apprenticeship;
    }

    public void DeletePayments(Guid apprenticeshipKey)
    {
        _sqlServerClient.Execute($"DELETE FROM [Domain].[Payment] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");
        _sqlServerClient.Execute($"DELETE FROM [Domain].[Earning] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");
        _sqlServerClient.Execute($"DELETE FROM [Domain].[Apprenticeship] WHERE ApprenticeshipKey = '{apprenticeshipKey}'");
    }

    /// <summary>
    /// When this is instatiated it will use an existing sql server client if one exists for the connection string,
    /// otherwise it will create a new one
    /// </summary>
    public PaymentsSqlClient()
    {
        var connectionString = Configurator.GetConfiguration().PaymentsDbConnectionString;
        _sqlServerClient = SqlServerClientProvider.GetSqlServerClient(connectionString);
    }
}
