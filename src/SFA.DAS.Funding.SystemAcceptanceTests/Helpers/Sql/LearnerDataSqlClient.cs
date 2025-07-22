﻿namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql
{
    public class LearnerDataSqlClient
    {
        private readonly SqlServerClient _sqlServerClient;

        public LearnerDataSqlClient()
        {
            var connectionString = Configurator.GetConfiguration().LearnerDbConnectionString;
            _sqlServerClient = SqlServerClientProvider.GetSqlServerClient(connectionString);
        }

        public void DeleteAllLearnerData()
        {
            _sqlServerClient.Execute($"DELETE FROM [dbo].[LearnerData]");
        }

        public LearnerData? GetLearnerData(long uln)
        {
            return
                _sqlServerClient.GetList<LearnerData>($"SELECT * FROM [dbo].[LearnerData] WHERE [ULN] = {@uln}")
                .FirstOrDefault();
        }

        public class LearnerData
        {
            public long ULN { get; set; }
            public long UKPRN { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public DateTime DoB { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime PlannedEndDate { get; set; }
            public int? PercentageLearningToBeDelivered { get; set; }
            public int EpaoPrice { get; set; }
            public int TrainingPrice { get; set; }
            public string? AgreementId { get; set; }
            public bool IsFlexiJob { get; set; }
            public int? PlannedOTJTrainingHours { get; set; }
            public int StandardCode { get; set; }
            public string ConsumerReference { get; set; }
        }
    }
}
