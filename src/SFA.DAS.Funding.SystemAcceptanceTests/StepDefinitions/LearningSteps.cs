using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class LearningSteps (ScenarioContext context, LearningSqlClient learningSqlClient)
    {
        [Then("all approved learners for the provider are returned in the response")]
        public async Task AllApprovedLearnersForTheProviderAreReturned()
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() => learningSqlClient.GetApprovedLearners(Constants.UkPrn, Convert.ToInt16(TableExtensions.CalculateAcademicYear("0"))) != null, "Unable to find Learners for Ukprn");

            var expectedLearners = learningSqlClient.GetApprovedLearners(Constants.UkPrn, Convert.ToInt16(TableExtensions.CalculateAcademicYear("0")));

            Assert.IsNotNull(expectedLearners);

            var actualLearners = testData.LearnersOnService;

            bool allExist = actualLearners.Learners
                .All(l1 => expectedLearners.Any(l2 => l2.Uln == l1.Uln && l2.Key == l1.Key));

            Assert.IsTrue(allExist, "Some learners in LearnerData outer response do not match with learners in learning db");
        }
    }
}
