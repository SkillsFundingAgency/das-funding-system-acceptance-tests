using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers.Data;

/// <summary>
/// Learning type is a concept in learning domain, but SLD supply us with a course code, which via the courses API called from 
/// Learner data outer resolves to a learning type.
/// </summary>
internal static class LearningTypeHelper
{
    internal static int GetStandardCode(string learningType)
    {
        return Parse(learningType) switch
        {
            LearningType.Apprenticeship => 614,
            LearningType.FoundationApprenticeship => 811,
            _ => throw new ArgumentOutOfRangeException(nameof(learningType), learningType, "Unsupported learningType")
        };
    }

    internal static LearningType Parse(string learningType)
    {
        if (Enum.TryParse<LearningType>(learningType, true, out var parsed))
        {
            return parsed;
        }

        throw new ArgumentException($"Unsupported learningType '{learningType}'", nameof(learningType));
    }
}
