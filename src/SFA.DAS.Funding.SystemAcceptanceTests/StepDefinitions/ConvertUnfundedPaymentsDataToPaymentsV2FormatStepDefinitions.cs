﻿using Microsoft.Azure.Amqp.Framing;
using NUnit.Framework.Internal;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using CMT = SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow.CommonModels;

namespace SFA.DAS.Funding.SystemAcceptanceTests.StepDefinitions
{
    [Binding]
    public class ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions
    {
        private readonly ScenarioContext _context;
        private readonly AdaptorMessageHandler _adaptorMessageHelper;
        private List<FinalisedOnProgammeLearningPaymentEvent> _finalisedPaymentsList;
        private List<CalculatedRequiredLevyAmount> _calculatedRequiredLevyAmountList;

        public ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _adaptorMessageHelper = new AdaptorMessageHandler(_context);
        }

        [Then(@"the Calculated Required Levy Amount event is published with default values")]
        public async Task ThenTheCalculatedRequiredLevyAmountEventIsPublishedWithDefaultValues()
        {
            await _adaptorMessageHelper.ReceiveCalculatedRequiredLevyAmountEvent(_context.Get<ApprenticeshipCreatedEvent>().Uln);

            _finalisedPaymentsList = _context.Get<List<FinalisedOnProgammeLearningPaymentEvent>>();
            _calculatedRequiredLevyAmountList = _context.Get<List<CalculatedRequiredLevyAmount>>();

            Assert.That(_finalisedPaymentsList.Count, Is.EqualTo(_calculatedRequiredLevyAmountList.Count), 
                "The count for FinalisedOnProgrammeLearningPaymentEvent does not match with CalculatedRequiredLevyAmount.");

            //remove below logging before creating PR. 
            for (int i = 0; i < _calculatedRequiredLevyAmountList.Count; i++)
            {
                Console.WriteLine("Printing CalculatedRequiredLevyAmount object an index {0}", i);
                PrintObjectProperties(_calculatedRequiredLevyAmountList[i]);
            }
            var apprenticeshipCreatedEvent = _context.Get<CMT.ApprenticeshipCreatedEvent>();

            for (int i = 0; i < _calculatedRequiredLevyAmountList.Count; i++)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(_calculatedRequiredLevyAmountList[i].Priority, Is.Zero, "Incorrect Priority found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].AgreementId, Is.Null, "Incorrect AgreementId found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].AgreedOnDate, Is.Null, "Incorrect Agreed On Date found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].OnProgrammeEarningType, Is.EqualTo(OnProgrammeEarningType.Learning), "Incorrect OnProgramme Earning Type found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].TransactionType, Is.EqualTo(TransactionType.Learning), "Incorrect Transaction Type found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].ClawbackSourcePaymentEventId, Is.Null, "Incorrect Clawback Source PAyment Event Id found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].PriceEpisodeIdentifier, Is.Empty, "Incorrect Price Episode Identifier found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].ContractType, Is.EqualTo(ContractType.Act1), "Incorrect Contract Type found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].ActualEndDate, Is.Null, "Incorrect Actual End Date found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].CompletionStatus, Is.EqualTo(1), "Incorrect Completion Status found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].CompletionAmount, Is.Zero, "Incorrect Completion Amount found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].ApprenticeshipPriceEpisodeId, Is.Null, "Incorrect Apprenticeship Price Episode Id");
                    Assert.That(_calculatedRequiredLevyAmountList[i].ReportingAimFundingLineType, Is.Empty, "Incorrect Reporting Aim Funding Line Type");
                    Assert.That(_calculatedRequiredLevyAmountList[i].LearningAimSequenceNumber, Is.Zero, "Incorrect Learning Aim Sequence Number found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].EventTime.DateTime, Is.GreaterThanOrEqualTo(DateTime.Now.AddMinutes(-1)).And.LessThanOrEqualTo(DateTime.Now), "Incorrect Event Time found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].EventId, Is.Not.Null.And.TypeOf<Guid>(), "Incorrect Event Id type found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].Learner.ReferenceNumber, Is.Null, "Incorrect Learner - Reference Number found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.Reference, Is.EqualTo("ZPROG001"), "Incorrect - Learning Aim - Reference found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.ProgrammeType, Is.Zero, "Incorrect Learning Aim - Programme Type found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.FrameworkCode, Is.Zero, "Incorrect Learning Aim - Framework code found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].AmountDue, Is.EqualTo(_finalisedPaymentsList[i].Amount), "Incorrect Amount Due found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].DeliveryPeriod, Is.EqualTo(_finalisedPaymentsList[i].ApprenticeshipEarnings.DeliveryPeriod), "Incorrect Delivery Period found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].StartDate, Is.EqualTo(_finalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Start Date found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].PlannedEndDate, Is.EqualTo(_finalisedPaymentsList[i].ActualEndDate), "Incorrect Planned End Date found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].LearningStartDate, Is.EqualTo(_finalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Learnings Start Date found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].Learner.Uln, Is.EqualTo(_finalisedPaymentsList[i].ApprenticeshipEarnings.Uln), "Incorrect ULN found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.FundingLineType, Is.Empty, "Incorrect Learning Aim - Funding Line Type found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.StartDate, Is.EqualTo(_finalisedPaymentsList[i].Apprenticeship.StartDate), "Incorrect Learning Aim - Start Date found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].CollectionPeriod.AcademicYear, Is.EqualTo(_finalisedPaymentsList[i].CollectionYear), "Incorrect Collection Period - Acadmic Year found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].CollectionPeriod.Period, Is.EqualTo(_finalisedPaymentsList[i].CollectionMonth), "Incorrect Collection Period - Period found");

                    Assert.That(_calculatedRequiredLevyAmountList[i].IlrFileName, Is.Empty, "Incorrect Irl File Name found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].IlrSubmissionDateTime, Is.EqualTo($"01/08/{TableExtensions.CalculateAcademicYear("CurrentMonth+0")}"), "Incorrect Ilr Submission Date Time found");

                    Assert.That(_calculatedRequiredLevyAmountList[i].JobId, Is.EqualTo(-1), "Incorrect Job Id found");

                    Assert.That(_calculatedRequiredLevyAmountList[i].SfaContributionPercentage, Is.EqualTo(0.95), "Incorrect Sfa Contribution Percentage found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].EarningEventId, Is.TypeOf<Guid>(), "Incorrect Earnings Event Id found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].TransferSenderAccountId, Is.EqualTo(apprenticeshipCreatedEvent.AccountId), "Incorrect Transfer Sender Account Id found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].InstalmentAmount, Is.EqualTo(_finalisedPaymentsList[i].Amount), "Incorrect Instalment Amount found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].NumberOfInstalments, Is.EqualTo(_finalisedPaymentsList[i].ApprenticeshipEarnings.NumberOfInstalments), "Incorrect Number of Installments found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].ApprenticeshipId, Is.EqualTo(_finalisedPaymentsList[i].ApprenticeshipKey), "Incorrect Apprenticeship Key found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].ApprenticeshipEmployerType, Is.EqualTo(ApprenticeshipEmployerType.Levy), "Incorrect Apprenticeship Employer Type found");
                    Assert.That(_calculatedRequiredLevyAmountList[i].LearningAim.StandardCode, Is.EqualTo(apprenticeshipCreatedEvent.TrainingCode), "Incorrect Learning Aim - Standard Code found");
                });
            }
        }
        public static void PrintObjectProperties(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                object propertyValue = property.GetValue(obj);
                if (property.PropertyType.Namespace.StartsWith("System"))
                {
                    Console.WriteLine("{0}: {1}", propertyName, propertyValue);
                }
                else
                {
                    Console.WriteLine("{0}:", propertyName);
                    PrintObjectProperties(propertyValue);
                }
            }
        }
    }
}
