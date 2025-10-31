﻿
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Events;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Extensions;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    internal class PersonalDetailsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly LearningSqlClient _apprenticeshipSqlClient;

        public PersonalDetailsStepDefinitions(ScenarioContext context, LearningSqlClient apprenticeshipSqlClient)        {
            _context = context;
            _apprenticeshipSqlClient = apprenticeshipSqlClient;
        }

        [When("Learner's personal details are updated with first name (.*) last name (.*) and email (.*)")]
        public void LearnersPersonalDetailsAreUpdated(string firstName, string lastName, string? email)
        {
            if (string.Equals(email, "null", StringComparison.OrdinalIgnoreCase))
            {
                email = null;
            }

            var testData = _context.Get<TestData>();
            var learnerDataBuilder = testData.GetLearnerDataBuilder();
            learnerDataBuilder.WithLearnersPersonalDetails(firstName, lastName, email);
        }

        [Then("Learner's personal details are updated in learning db with first name (.*) last name (.*) and email (.*)")]
        public void LearnersPersonalDetailsAreUpdatedInLearningDb(string firstName, string lastName, string? email)
        {
            if (string.Equals(email, "null", StringComparison.OrdinalIgnoreCase))
            {
                email = null;
            }

            var testData = _context.Get<TestData>();
            SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql.Learning? apprenticeship = null;

            apprenticeship = _apprenticeshipSqlClient.GetApprenticeship(testData.LearningKey);

            
                Assert.AreEqual(firstName, apprenticeship.FirstName, "Unexpected First Name found!");
                Assert.AreEqual(lastName, apprenticeship.LastName, "Unexpected Last Name found");
                Assert.AreEqual(email, apprenticeship.EmailAddress, "Unexpected Email address found");
        }


        [Then("a personal details changed event is published to approvals with first name (.*) last name (.*) and email (.*)")]
        public async Task PersonalDetailsChangedEventIsPublishedToApprovals(string firstName, string lastName, string? email)
        {
            if (string.Equals(email, "null", StringComparison.OrdinalIgnoreCase))
            {
                email = null;
            }

            var testData = _context.Get<TestData>();
            await _context.ReceivePersonalDetailsChangedEvent(testData.LearningKey);

            
                Assert.AreEqual(firstName, testData.PersonalDetailsChangedEvent.FirstName, "Unexpected First Name found!");
                Assert.AreEqual(lastName, testData.PersonalDetailsChangedEvent.LastName, "Unexpected Last Name found");
                Assert.AreEqual(email, testData.PersonalDetailsChangedEvent.EmailAddress, "Unexpected Email address found");
            
        }

    }
}
