﻿using SFA.DAS.Payments.Messages.Core.Events;
using SFA.DAS.Payments.Model.Core;
using SFA.DAS.Payments.Model.Core.Entities;
using SFA.DAS.Payments.Model.Core.OnProgramme;

namespace SFA.DAS.Payments.RequiredPayments.Messages.Events
{
    public class CalculatedRequiredLevyAmount : IPaymentsEvent
    {
        public string EarningSource => "SubmitLearnerDataFundingPlatform"; // new field
        public int Priority { get; set; }
        public string AgreementId { get; set; }
        public DateTime? AgreedOnDate { get; set; }
        public decimal SfaContributionPercentage { get; set; }
        public OnProgrammeEarningType OnProgrammeEarningType { get; set; }
        public TransactionType TransactionType => TransactionType.Learning;
        public Guid EarningEventId { get; set; }
        public Guid? ClawbackSourcePaymentEventId { get; set; }
        public string PriceEpisodeIdentifier { get; set; }
        public decimal AmountDue { get; set; }
        public byte DeliveryPeriod { get; set; }
        public long? AccountId { get; set; }
        public long? TransferSenderAccountId { get; set; }
        public ContractType ContractType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? PlannedEndDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public byte CompletionStatus { get; set; }
        public decimal CompletionAmount { get; set; }
        public decimal InstalmentAmount { get; set; }
        public short NumberOfInstalments { get; set; }
        public DateTime? LearningStartDate { get; set; }
        public long? ApprenticeshipId { get; set; }
        public long? ApprenticeshipPriceEpisodeId { get; set; }
        public ApprenticeshipEmployerType ApprenticeshipEmployerType { get; set; }
        public string ReportingAimFundingLineType { get; set; }
        public long? LearningAimSequenceNumber { get; set; }
        public long JobId { get; set; }
        public DateTimeOffset EventTime { get; set; }
        public Guid EventId { get; set; }
        public long Ukprn { get; set; }
        public Learner Learner { get; set; } = new();
        public LearningAim LearningAim { get; set; } = new();
        public DateTime IlrSubmissionDateTime { get; set; }
        public string IlrFileName { get; set; }
        public CollectionPeriod CollectionPeriod { get; set; } = new();
    }
}