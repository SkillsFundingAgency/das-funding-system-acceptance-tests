﻿
namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport
{
    public class PaymentsEntityModel
    {
        public PaymentsModel Model { get; set; }
    }

    public class PaymentsModel
    {
        public Guid ApprenticeshipKey { get; set; }
        public Earnings[] Earnings { get; set;}
        public Payments[] Payments { get; set; }
    }

    public class Earnings
    {
        public int DeliveryPeriod { get; set; }
        public int AcademicYear { get; set; }
        public int CollectionMonth { get; set; }
        public int CollectionYear { get; set; }
        public double Amount { get; set; }
    }

    public class Payments
    {
        public int AcademicYear { get; set; }
        public int DeliveryPeriod { get; set; }
        public double Amount { get; set; }
        public int PaymentYear { get; set; }
        public int PaymentPeriod { get; set; }
        public bool SentForPayment { get; set; }
    }
}