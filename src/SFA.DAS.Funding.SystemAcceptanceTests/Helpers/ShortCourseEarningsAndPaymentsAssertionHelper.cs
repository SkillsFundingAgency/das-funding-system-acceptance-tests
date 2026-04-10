using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Payments.EarningEvents.Messages.External;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public class ShortCourseEarningsAndPaymentsAssertionHelper(ScenarioContext context, EarningsSqlClient earningsSqlClient)
    {
        public async Task AssertAllEarningsRemoved()
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;

                var dbValid = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.All(x => !x.IsPayable) ?? true;
                var commandValid = paymentsCommand != null && !paymentsCommand.Earnings.Any();

                return dbValid && commandValid;
            }, "Failed to verify that all earnings were removed in the earnings db & on the command sent to payments.");
        }

        public async Task AssertRemainingCompletionEarningRemoved()
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
                var dbValid = instalments != null && instalments.All(x => x.Type != "LearningComplete" || !x.IsPayable);

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null && paymentsCommand.Earnings.All(e => e.PricePeriods.All(p => p.Periods.All(ep => ep.EarningType != EarningType.Completion)));

                return dbValid && commandValid;
            }, "Failed to verify that the completion earning was removed in the earnings db & on the command send to payments.");
        }

        public async Task Assert30PercentMilestoneEarningRemoved()
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
                var dbValid = instalments != null && instalments.All(x => x.Type != "ThirtyPercentLearningComplete" || !x.IsPayable);

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert the 30% milestone earning is removed from the sent command

                return dbValid && commandValid;
            }, "Failed to verify that the 30% milestone earning was removed in the earnings db & on the command send to payments.");
        }

        public async Task Assert30PercentMilestoneEarningRetained()
        {
            var testData = context.Get<TestData>();

            ShortCourseEarningsModel? earningsModel = null;
            await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
                var dbValid = instalments != null && instalments.Any(x => x.Type == "ThirtyPercentLearningComplete" && x.IsPayable);

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert the 30% milestone earning is retained/generated in the sent command

                return dbValid && commandValid;
            }, "Failed to find short course earnings with 30% milestone earning in the earnings db & on the command send to payments.");

            var instalment = earningsModel!.Episodes.Single().EarningsProfile.Instalments.Single(x => x.Type == "ThirtyPercentLearningComplete" && x.IsPayable);
            var totalPrice = earningsModel.Episodes.Single().CoursePrice;
            var expectedAmount = Math.Round(totalPrice * 0.3m, 5);

            Assert.AreEqual((double)expectedAmount, (double)instalment.Amount, 0.01, "The retained instalment is not the 30% milestone earning.");
        }

        public async Task AssertCompletionEarningGenerated()
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
                var dbValid = instalments != null && instalments.Any(x => x.Type == "LearningComplete" && x.IsPayable);

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert the completion earning is generated in the sent command

                return dbValid && commandValid;
            }, "Failed to verify that the completion earning was generated in the earnings db & on the command send to payments.");
        }

        public async Task AssertBasicShortCourseEarningsGenerated()
        {
            var testData = context.Get<TestData>();

            var expectedCourse = testData.ShortCourseLearnerData!.Delivery.OnProgramme.Single();
            var expectedStartDate = expectedCourse.StartDate;
            var expectedEndDate = expectedCourse.ExpectedEndDate;

            ShortCourseEarningsModel? earningsModel = null;
            await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var dbValid = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert the basic earnings are generated correctly in the sent command

                return dbValid && commandValid;
            }, "Failed to find short course earnings entity in the earnings db & on the command send to payments.");

            var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;

            var duration = (expectedEndDate - expectedStartDate).Days + 1;
            var daysToFirstPayment = (int)Math.Floor(duration * 0.3);
            var firstPaymentDate = expectedStartDate.AddDays(daysToFirstPayment);
            var secondPaymentDate = expectedEndDate;

            var expectedFirstPeriod = TableExtensions.Period[firstPaymentDate.ToString("MMMM")];
            var expectedFirstAcademicYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", firstPaymentDate));

            var expectedSecondPeriod = TableExtensions.Period[secondPaymentDate.ToString("MMMM")];
            var expectedSecondAcademicYear = Convert.ToInt16(TableExtensions.CalculateAcademicYear("0", secondPaymentDate));

            var firstInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedFirstPeriod && x.AcademicYear == expectedFirstAcademicYear);
            Assert.IsNotNull(firstInstalment, $"Could not find first instalment in period {expectedFirstPeriod} of AY {expectedFirstAcademicYear}");

            var secondInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedSecondPeriod && x.AcademicYear == expectedSecondAcademicYear);
            Assert.IsNotNull(secondInstalment, $"Could not find second instalment in period {expectedSecondPeriod} of AY {expectedSecondAcademicYear}");

            var totalPrice = earningsModel.Episodes.Single().CoursePrice;

            var expectedFirstAmount = Math.Round(totalPrice * 0.3m, 5);
            var expectedSecondAmount = Math.Round(totalPrice * 0.7m, 5);

            Assert.AreEqual((double)expectedFirstAmount, (double)firstInstalment.Amount, 0.01, "First instalment amount does not match exactly 30% of total price.");
            Assert.AreEqual((double)expectedSecondAmount, (double)secondInstalment.Amount, 0.01, "Second instalment amount does not match exactly 70% of total price.");
        }

        public async Task AssertShortCourseEarningsSetToApproved()
        {
            var testData = context.Get<TestData>();

            ShortCourseEarningsModel? earningsModel = null;
            await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var dbValid = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile != null;

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert the earnings are approved in the sent command if applicable

                return dbValid && commandValid;
            }, "Failed to find short course earnings entity in the earnings db & on the command send to payments.");

            Assert.IsTrue(earningsModel!.Episodes.Single().EarningsProfile.IsApproved, "Short course earnings should be approved.");
        }

        public async Task AssertShortCourseEarningsAreGeneratedWithoutDuplication()
        {
            var testData = context.Get<TestData>();
            ShortCourseEarningsModel? earningsModel = null;
            await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var dbValid = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert the earnings do not contain duplicates in the sent command

                return dbValid && commandValid;
            }, "Failed to find short course earnings entity in the earnings db & on the command send to payments.");
            var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;
            Assert.AreEqual(2, instalments.Count, "Expected exactly 2 instalments for the short course, but found a different count.");
        }

        public async Task AssertSecondInstalmentIsEarntInPeriod(TokenisablePeriod period)
        {
            var testData = context.Get<TestData>();

            ShortCourseEarningsModel? earningsModel = null;
            await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var dbValid = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.Count == 2;

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert the second instalment is in the correct period in the sent command

                return dbValid && commandValid;
            }, "Failed to find short course earnings entity in the earnings db & on the command send to payments.");

            var instalments = earningsModel!.Episodes.Single().EarningsProfile.Instalments;

            var expectedSecondPeriod = period.Value.PeriodValue;
            var expectedSecondAcademicYear = period.Value.AcademicYear;

            var secondInstalment = instalments.SingleOrDefault(x => x.DeliveryPeriod == expectedSecondPeriod && x.AcademicYear == expectedSecondAcademicYear);
            Assert.IsNotNull(secondInstalment, $"Could not find second instalment in period {expectedSecondPeriod} of AY {expectedSecondAcademicYear}");

            var totalPrice = earningsModel.Episodes.Single().CoursePrice;
            var expectedSecondAmount = Math.Round(totalPrice * 0.7m, 5);

            Assert.AreEqual((double)expectedSecondAmount, (double)secondInstalment.Amount, 0.01, "Second instalment amount (70% of total price) not found in expected delivery period.");
        }

        public async Task AssertOnlyEarningsAreGeneratedForTheEarliestShortCourse()
        {
            var testData = context.Get<TestData>();
            
            ShortCourseEarningsModel? earningsModel = null;
            await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var dbValid = earningsModel?.Episodes?.Count > 0;

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert exactly 1 episode is generated for the earliest course in the sent command

                return dbValid && commandValid;
            }, "Failed to find short course earnings entity in the earnings db & on the command send to payments.");

            Assert.AreEqual(1, earningsModel!.Episodes.Count, "Expected exactly 1 episode for the earliest short course earnings, but found a different count.");
        }

        public async Task AssertEarningsAreStillRecordedAgainstTheFirstProvider()
        {
            var testData = context.Get<TestData>();
            
            ShortCourseEarningsModel? earningsModel = null;
            await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var dbValid = earningsModel?.Episodes?.Count > 0;

                var paymentsCommand = GrowthAndSkillsPaymentsRecalculatedEventHandler.GetMessage(x => x.Command.Learner.LearnerKey == testData.ShortCourseLearningKey)?.Command;
                var commandValid = paymentsCommand != null; // TODO: assert the earnings are recorded against the first provider in the sent command

                return dbValid && commandValid;
            }, "Failed to find short course earnings entity in the earnings db & on the command send to payments.");

            Assert.AreEqual(Constants.UkPrn, earningsModel!.Episodes.Single().Ukprn, "The earnings were not recorded against the first provider's UKPRN.");
        }
    }
}
