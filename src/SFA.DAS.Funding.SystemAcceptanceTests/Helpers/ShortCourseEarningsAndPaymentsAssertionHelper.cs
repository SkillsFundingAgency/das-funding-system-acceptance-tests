using NUnit.Framework.Interfaces;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Sql;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Payments.EarningEvents.Messages.External;
using SFA.DAS.Payments.EarningEvents.Messages.External.Commands;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public class ShortCourseEarningsAndPaymentsAssertionHelper(ScenarioContext context, EarningsSqlClient earningsSqlClient, LearningSqlClient learningSqlClient)
    {
        public async Task AssertAllEarningsRemoved()
        {
            await WaitHelper.WaitForIt(() =>
            {
                var testData = context.Get<TestData>();

                var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var dbValid = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.All(x => !x.IsPayable) ?? true;

                var commandValid = AssertAgainstPaymentsCommand(c => !c.Earnings.Any());

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to verify that all earnings were removed in the earnings db.");
        }

        public async Task AssertRemainingCompletionEarningRemoved()
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
                var dbValid = instalments != null && instalments.All(x => x.Type != "LearningComplete" || !x.IsPayable);

                var commandValid = AssertAgainstPaymentsCommand(c => c.Earnings.All(e => e.PricePeriods.All(p => p.Periods.All(ep => ep.EarningType != EarningType.Completion))));

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to verify that the completion earning was removed in the earnings db.");
        }

        public async Task Assert30PercentMilestoneEarningRemoved()
        {
            var testData = context.Get<TestData>();

            await WaitHelper.WaitForIt(() =>
            {
                var earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
                var dbValid = instalments != null && instalments.All(x => x.Type != "ThirtyPercentLearningComplete" || !x.IsPayable);

                var commandValid = AssertAgainstPaymentsCommand(c => c.Earnings.All(e => e.PricePeriods.All(p => p.Periods.All(ep => ep.EarningType != EarningType.Milestone1))));

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to verify that the 30% milestone earning was removed in the earnings db.");
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

                var commandValid = AssertAgainstPaymentsCommand(c => c.Earnings.Any(e => e.PricePeriods.Any(p => p.Periods.Any(ep => ep.EarningType == EarningType.Milestone1))));

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to find short course earnings with 30% milestone earning in the earnings db.");

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

                var commandValid = AssertAgainstPaymentsCommand(c => c.Earnings.Any(e => e.PricePeriods.Any(p => p.Periods.Any(ep => ep.EarningType == EarningType.Completion))));

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to verify that the completion earning was generated in the earnings db.");
        }

        public async Task AssertMilestoneAndCompletionEarningsStatus(string milestoneStatus, string completionStatus)
        {
            var testData = context.Get<TestData>();
            
            bool milestoneExpected = milestoneStatus == "generated";
            bool completionExpected = completionStatus == "generated";

            ShortCourseEarningsModel? earningsModel = null;
            await WaitHelper.WaitForIt(() =>
            {
                earningsModel = earningsSqlClient.GetShortCourseEarningsEntityModel(testData.Uln);
                var instalments = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments;
                
                bool dbMilestoneValid = false;
                bool dbCompletionValid = false;
                
                if (instalments != null)
                {
                    dbMilestoneValid = milestoneExpected 
                        ? instalments.Any(x => x.Type == "ThirtyPercentLearningComplete" && x.IsPayable) 
                        : instalments.All(x => x.Type != "ThirtyPercentLearningComplete" || !x.IsPayable);

                    dbCompletionValid = completionExpected 
                        ? instalments.Any(x => x.Type == "LearningComplete" && x.IsPayable) 
                        : instalments.All(x => x.Type != "LearningComplete" || !x.IsPayable);
                }
                else if (!milestoneExpected && !completionExpected)
                {
                    var dbValidNoMilestones = earningsModel?.Episodes?.FirstOrDefault()?.EarningsProfile?.Instalments?.All(x => !x.IsPayable) ?? true;
                    dbMilestoneValid = dbValidNoMilestones;
                    dbCompletionValid = dbValidNoMilestones;
                }

                var dbValid = dbMilestoneValid && dbCompletionValid;

                var commandValid = false;

                if (testData.ExpectGrowthAndSkillsPaymentsEvent)
                {
                    var learnerKey = learningSqlClient.GetShortCourseLearning(testData.Uln)?.Learner.Key;

                    testData.CalculateGrowthAndSkillsPaymentsCommand =
                        GrowthAndSkillsPaymentsRecalculatedEventHandler
                            .GetMessage(x => x.Command.Learner.LearnerKey == learnerKey)
                            ?.Command
                        ?? testData.CalculateGrowthAndSkillsPaymentsCommand;

                    if (testData.CalculateGrowthAndSkillsPaymentsCommand != null)
                    {
                        var commandMilestoneValid = milestoneExpected
                            ? testData.CalculateGrowthAndSkillsPaymentsCommand.Earnings.Any(e => e.PricePeriods.Any(p => p.Periods.Any(ep => ep.EarningType == EarningType.Milestone1)))
                            : testData.CalculateGrowthAndSkillsPaymentsCommand.Earnings.All(e => e.PricePeriods.All(p => p.Periods.All(ep => ep.EarningType != EarningType.Milestone1)));

                        var commandCompletionValid = completionExpected
                            ? testData.CalculateGrowthAndSkillsPaymentsCommand.Earnings.Any(e => e.PricePeriods.Any(p => p.Periods.Any(ep => ep.EarningType == EarningType.Completion)))
                            : testData.CalculateGrowthAndSkillsPaymentsCommand.Earnings.All(e => e.PricePeriods.All(p => p.Periods.All(ep => ep.EarningType != EarningType.Completion)));

                        commandValid = commandMilestoneValid && commandCompletionValid;
                    }
                }

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, $"Failed to verify that milestone is '{milestoneStatus}' and completion is '{completionStatus}' in the earnings db.");

            if (milestoneExpected)
            {
                var instalment = earningsModel!.Episodes.Single().EarningsProfile.Instalments.Single(x => x.Type == "ThirtyPercentLearningComplete" && x.IsPayable);
                var totalPrice = earningsModel.Episodes.Single().CoursePrice;
                var expectedAmount = Math.Round(totalPrice * 0.3m, 5);

                Assert.AreEqual((double)expectedAmount, (double)instalment.Amount, 0.01, "The retained instalment is not the 30% milestone earning.");
            }
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

                var commandValid = AssertAgainstPaymentsCommand(c => c.Earnings.Any(e => e.PricePeriods.Any(p => p.Periods.Any(ep => ep.EarningType == EarningType.Milestone1)))
                    && c.Earnings.Any(e => e.PricePeriods.Any(p => p.Periods.Any(ep => ep.EarningType == EarningType.Completion))));

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to find short course earnings entity in the earnings db.");

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

                var commandValid = AssertAgainstPaymentsCommand(c => true);

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to find short course earnings entity in the earnings db.");

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

                var commandValid = AssertAgainstPaymentsCommand(c => 
                {
                    var periods = c.Earnings.SelectMany(e => e.PricePeriods).SelectMany(p => p.Periods).ToList();
                    return periods.Count == periods.DistinctBy(p => new { p.DeliveryPeriod, p.EarningType }).Count();
                });

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to find short course earnings entity in the earnings db.");
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

                var completionPeriod = period.Value.PeriodValue;
                var commandValid = AssertAgainstPaymentsCommand(c => c.Earnings.SelectMany(e => e.PricePeriods).SelectMany(p => p.Periods).Any(ep => ep.EarningType == EarningType.Completion && ep.DeliveryPeriod == completionPeriod));

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to find short course earnings entity in the earnings db.");

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

                var commandValid = AssertAgainstPaymentsCommand(c => c.Earnings.Count() == 1);

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to find short course earnings entity in the earnings db.");

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

                var commandValid = AssertAgainstPaymentsCommand(c => c.UKPRN == Constants.UkPrn);

                return dbValid && (!testData.ExpectGrowthAndSkillsPaymentsEvent || commandValid);
            }, "Failed to find short course earnings entity in the earnings db.");

            Assert.AreEqual(Constants.UkPrn, earningsModel!.Episodes.Single().Ukprn, "The earnings were not recorded against the first provider's UKPRN.");
        }

        private bool AssertAgainstPaymentsCommand(Func<CalculateGrowthAndSkillsPayments, bool> predicate)
        {
            var testData = context.Get<TestData>();

            var commandValid = testData.CalculateGrowthAndSkillsPaymentsCommand != null && predicate.Invoke(testData.CalculateGrowthAndSkillsPaymentsCommand);

            if (!commandValid)
            {
                var learnerKey = learningSqlClient.GetShortCourseLearning(testData.Uln)?.Learner.Key;
                testData.CalculateGrowthAndSkillsPaymentsCommand =
                    GrowthAndSkillsPaymentsRecalculatedEventHandler
                        .GetMessage(x => x.Command.Learner.LearnerKey == learnerKey)
                        ?.Command
                    ?? testData.CalculateGrowthAndSkillsPaymentsCommand;
                commandValid = testData.CalculateGrowthAndSkillsPaymentsCommand != null && predicate.Invoke(testData.CalculateGrowthAndSkillsPaymentsCommand);
            }

            return commandValid;
        }
    }
}
