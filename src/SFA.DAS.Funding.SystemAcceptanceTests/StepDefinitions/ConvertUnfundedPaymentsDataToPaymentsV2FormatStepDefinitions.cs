using Microsoft.Azure.Amqp.Framing;
using NUnit.Framework.Internal;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.ApprenticeshipPayments.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.Helpers;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
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

        public ConvertUnfundedPaymentsDataToPaymentsV2FormatStepDefinitions(ScenarioContext context)
        {
            _context = context;
            _adaptorMessageHelper = new AdaptorMessageHandler(_context);
        }

        [Then(@"the Calculated Required Levy Amount event is published with default values")]
        public async Task ThenTheCalculatedRequiredLevyAmountEventIsPublishedWithDefaultValues()
        {
            await _adaptorMessageHelper.ReceiveCalculatedRequiredLevyAmountEvent(_context.Get<ApprenticeshipCreatedEvent>().Uln);

            var calculatedRequiredLevyAmount = _context.Get<CalculatedRequiredLevyAmount>();

            _finalisedPaymentsList = _context.Get<List<FinalisedOnProgammeLearningPaymentEvent>>();

            foreach ( var payment in _finalisedPaymentsList)
            {
                Assert.Multiple(() =>
                {
                    Assert.That(calculatedRequiredLevyAmount.Priority, Is.Zero, "Incorrect Priority found");
                    Assert.That(calculatedRequiredLevyAmount.AgreementId, Is.Null, "Incorrect AgreementId found");
                    Assert.That(calculatedRequiredLevyAmount.AgreedOnDate, Is.Null, "Incorrect Agreed On Date found");
                    Assert.That(calculatedRequiredLevyAmount.OnProgrammeEarningType, Is.EqualTo(OnProgrammeEarningType.Learning), "Incorrect OnProgramme Earning Type found");
                    Assert.That(calculatedRequiredLevyAmount.TransactionType, Is.EqualTo(TransactionType.Learning), "Incorrect Transaction Type found");
                    Assert.That(calculatedRequiredLevyAmount.ClawbackSourcePaymentEventId, Is.Null, "Incorrect Clawback Source PAyment Event Id found");
                    Assert.That(calculatedRequiredLevyAmount.PriceEpisodeIdentifier, Is.Empty, "Incorrect Price Episode Identifier found");
                    Assert.That(calculatedRequiredLevyAmount.ContractType, Is.EqualTo(ContractType.Act1), "Incorrect Contract Type found");
                    Assert.That(calculatedRequiredLevyAmount.ActualEndDate, Is.Null, "Incorrect Actual End Date found");
                    Assert.That(calculatedRequiredLevyAmount.CompletionStatus, Is.EqualTo(1), "Incorrect Completion Status found");
                    Assert.That(calculatedRequiredLevyAmount.CompletionAmount, Is.Zero, "Incorrect Completion Amount found");
                    Assert.That(calculatedRequiredLevyAmount.ApprenticeshipPriceEpisodeId, Is.Null, "Incorrect Apprenticeship Price Episode Id");
                    Assert.That(calculatedRequiredLevyAmount.ReportingAimFundingLineType, Is.Empty, "Incorrect Reporting Aim Funding Line Type");
                    Assert.That(calculatedRequiredLevyAmount.LearningAimSequenceNumber, Is.Zero, "Incorrect Learning Aim Sequence Number found");
                    //Assert.That(calculatedRequiredLevyAmount.EventTime.DateTime, Is.GreaterThanOrEqualTo(DateTime.Now.AddMinutes(-1)).And.LessThanOrEqualTo(DateTime.Now));
                    Assert.That(calculatedRequiredLevyAmount.EventId, Is.Not.Null.And.TypeOf<Guid>(), "Incorrect Event Id found");
                    Assert.That(calculatedRequiredLevyAmount.Learner.ReferenceNumber, Is.Null, "Incorrect Learner - Reference Number found");
                    Assert.That(calculatedRequiredLevyAmount.LearningAim.Reference, Is.EqualTo("ZPROG001"), "Incorrect - Learning Aim - Reference found");
                    Assert.That(calculatedRequiredLevyAmount.LearningAim.ProgrammeType, Is.Zero, "Incorrect Learning Aim - Programme Type found");
                    Assert.That(calculatedRequiredLevyAmount.LearningAim.FrameworkCode, Is.Zero, "Incorrect Learning Aim - Framework code found");
                    Assert.That(calculatedRequiredLevyAmount.AmountDue, Is.EqualTo(payment.Amount), "Incorrect Amount Due found");
                    Assert.That(calculatedRequiredLevyAmount.DeliveryPeriod, Is.EqualTo(payment.ApprenticeshipEarnings.DeliveryPeriod), "Incorrect Delivery Period found");
                    Assert.That(calculatedRequiredLevyAmount.StartDate, Is.EqualTo(payment.Apprenticeship.StartDate), "Incorrect Start Date found");
                    Assert.That(calculatedRequiredLevyAmount.PlannedEndDate, Is.EqualTo(payment.ActualEndDate), "Incorrect Planned End Date found");
                    Assert.That(calculatedRequiredLevyAmount.LearningStartDate, Is.EqualTo(payment.Apprenticeship.StartDate), "Incorrect Learnings Start Date found");
                    Assert.That(calculatedRequiredLevyAmount.Learner.Uln, Is.EqualTo(payment.ApprenticeshipEarnings.Uln), "Incorrect ULN found");
                    //  Assert.That(calculatedRequiredLevyAmount.LearningAim.FundingLineType, Is.Zero);
                    Assert.That(calculatedRequiredLevyAmount.LearningAim.StartDate, Is.EqualTo(payment.Apprenticeship.StartDate), "Incorrect Learning Aim - Start Date found");
                    Assert.That(calculatedRequiredLevyAmount.CollectionPeriod.AcademicYear, Is.EqualTo(payment.CollectionYear), "Incorrect Collection Period - Acadmic Year found");
                    Assert.That(calculatedRequiredLevyAmount.CollectionPeriod.Period, Is.EqualTo(payment.CollectionMonth), "Incorrect Collection Period - Period found");
                });
            }
        }
    }
}
