using Microsoft.Azure.Amqp.Framing;
using NUnit.Framework.Internal;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;
using SFA.DAS.Payments.RequiredPayments.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
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

            var properties = calculatedRequiredLevyAmount.GetType().GetProperties();

            for (int i = 0; i < properties.Length; i++)
            {
                Console.WriteLine("{0}: {1}", properties[i].Name, properties[i].GetValue(calculatedRequiredLevyAmount));
            }

                Assert.Multiple(() =>
            {
                Assert.That(calculatedRequiredLevyAmount.Priority, Is.Zero);
                Assert.That(calculatedRequiredLevyAmount.AgreementId, Is.Null);
                Assert.That(calculatedRequiredLevyAmount.AgreedOnDate, Is.Null);
                Assert.That(calculatedRequiredLevyAmount.OnProgrammeEarningType, Is.EqualTo(OnProgrammeEarningType.Learning));
                Assert.That(calculatedRequiredLevyAmount.TransactionType, Is.EqualTo(TransactionType.Learning));
                Assert.That(calculatedRequiredLevyAmount.ClawbackSourcePaymentEventId, Is.Null);
                Assert.That(calculatedRequiredLevyAmount.PriceEpisodeIdentifier, Is.Empty);
                Assert.That(calculatedRequiredLevyAmount.ContractType, Is.EqualTo(ContractType.Act1));
                Assert.That(calculatedRequiredLevyAmount.ActualEndDate, Is.Null);
                Assert.That(calculatedRequiredLevyAmount.CompletionStatus, Is.EqualTo(1));
                Assert.That(calculatedRequiredLevyAmount.CompletionAmount, Is.Zero);
                Assert.That(calculatedRequiredLevyAmount.ApprenticeshipPriceEpisodeId, Is.Null);
                Assert.That(calculatedRequiredLevyAmount.ReportingAimFundingLineType, Is.Empty);
                Assert.That(calculatedRequiredLevyAmount.LearningAimSequenceNumber, Is.Zero);
                //Assert.That(calculatedRequiredLevyAmount.EventTime.DateTime, Is.GreaterThanOrEqualTo(DateTime.Now.AddMinutes(-1)).And.LessThanOrEqualTo(DateTime.Now));
                Assert.That(calculatedRequiredLevyAmount.EventId, Is.Not.Null.And.TypeOf<Guid>());
                Assert.That(calculatedRequiredLevyAmount.Learner.ReferenceNumber, Is.Null);
                Assert.That(calculatedRequiredLevyAmount.LearningAim.Reference, Is.EqualTo("ZPROG001"));
                Assert.That(calculatedRequiredLevyAmount.LearningAim.ProgrammeType, Is.Zero);
                Assert.That(calculatedRequiredLevyAmount.LearningAim.FrameworkCode, Is.Zero);
            });
        }
    }
}
