namespace SFA.DAS.Funding.SystemAcceptanceTests.TestSupport;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class ShortCourseEarningsModel
{
    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string Uln { get; set; }
    public List<ShortCourseEpisodeModel> Episodes { get; set; }
}

public class ShortCourseEpisodeModel
{
    public Guid Key { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public string FundingType { get; set; }
    public long? FundingEmployerAccountId { get; set; }
    public string LegalEntityName { get; set; }
    public string TrainingCode { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal CoursePrice { get; set; }
    public ShortCourseEarningsProfileModel EarningsProfile { get; set; }
    public List<EarningsProfileHistoryModel> EarningsProfileHistory { get; set; }
}

public class ShortCourseEarningsProfileModel
{
    public Guid EarningsProfileId { get; set; }
    public Guid EpisodeKey { get; set; }
    public decimal OnProgramTotal { get; set; }
    public decimal? CompletionPayment { get; set; }
    public Guid? Version { get; set; }
    public bool IsApproved { get; set; }
    public List<ShortCourseInstalmentModel> Instalments { get; set; }
}

public class ShortCourseInstalmentModel : InstalmentModelBase
{
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
