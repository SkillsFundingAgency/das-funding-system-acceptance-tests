using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Http;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class MathAndEnglishAssertionsStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly EarningsSqlClient _earningsEntitySqlClient;


        public MathAndEnglishAssertionsStepDefinitions(ScenarioContext context, EarningsSqlClient earningsEntitySqlClient)
        {
            _context = context;
            _earningsEntitySqlClient = earningsEntitySqlClient;
        }

        [When("Maths and English learning is recorded from (.*) to (.*) with course (.*) and amount (.*)")]
        public async Task AddMathsAndEnglishLearning(TokenisableDateTime StartDate, TokenisableDateTime EndDate, string course, decimal amount)
        {
            var testData = _context.Get<TestData>();
            PaymentsGeneratedEventHandler.ReceivedEvents.Clear();
            var helper = new EarningsInnerApiHelper();
            await helper.SetMathAndEnglishLearning(_context.Get<ApprenticeshipCreatedEvent>().ApprenticeshipKey,
                new EarningsInnerApiClient.MathAndEnglishDetails() { StartDate = StartDate.Value, EndDate = EndDate.Value, Amount = amount, Course = course });
            testData.IsMathsAndEnglishAdded = true;
        }

        [Then("Maths and English earnings are generated from periods (.*) to (.*) with instalment amount (.*)")]
        public async Task VerifyMathsAndEnglishEarnings(TokenisablePeriod MathsAndEnglishStartPeriod, TokenisablePeriod MathsAndEnglishEndPeriod, decimal instalment)
        {
            var testData = _context.Get<TestData>();
            EarningsApprenticeshipModel? earningsApprenticeshipModel = null;

            await WaitHelper.WaitForIt(() =>
            {
                earningsApprenticeshipModel = _earningsEntitySqlClient.GetEarningsEntityModel(_context);
                return !testData.IsMathsAndEnglishAdded || earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfileHistory.Any();
            }, "Failed to find updated earnings entity.");

            var mathsAndEnglishInstalments = earningsApprenticeshipModel
                .Episodes
                .SingleOrDefault()
                ?.MathsAndEnglishInstalments;

            testData.EarningsProfileId = earningsApprenticeshipModel.Episodes.SingleOrDefault().EarningsProfile.EarningsProfileId;

            mathsAndEnglishInstalments.Should().NotBeNull("No Maths and English instalment data found on earnings apprenticeship model");

            while (MathsAndEnglishStartPeriod.Value.AcademicYear < MathsAndEnglishEndPeriod.Value.AcademicYear || MathsAndEnglishStartPeriod.Value.AcademicYear >= MathsAndEnglishEndPeriod.Value.AcademicYear && MathsAndEnglishStartPeriod.Value.PeriodValue <= MathsAndEnglishEndPeriod.Value.PeriodValue)
            {
                mathsAndEnglishInstalments.Should().Contain(x =>
                        x.Amount == instalment
                        && x.AcademicYear == MathsAndEnglishStartPeriod.Value.AcademicYear
                        && x.DeliveryPeriod == MathsAndEnglishStartPeriod.Value.PeriodValue, $"Expected Maths and English earning for {MathsAndEnglishStartPeriod.Value.ToCollectionPeriodString()}");

                MathsAndEnglishStartPeriod.Value = MathsAndEnglishStartPeriod.Value.GetNextPeriod();
            }
        }
    }
}
