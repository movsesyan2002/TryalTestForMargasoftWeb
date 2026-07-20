namespace TryalTestForMargsoftCore.Constants;

using TryalTestForMargsoftCore.Enums;

public static class RecommendedActions
{
    public const string NoAction = "No action";
    public const string Monitor = "Monitor";
    public const string RequestMoreInformation = "Request more information";
    public const string ReviewDenial = "Review denial";
    public const string FollowUpWithPayerProvider = "Follow up with payer/provider";
    public const string PrepareDemandLetter = "Prepare demand letter";
    public const string EscalateForAttorneyReview = "Escalate for attorney review";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        NoAction,
        Monitor,
        RequestMoreInformation,
        ReviewDenial,
        FollowUpWithPayerProvider,
        PrepareDemandLetter,
        EscalateForAttorneyReview
    };

    /// <summary>
    /// Returns whether the supplied database value is a supported recommendation action.
    /// </summary>
    public static bool IsValid(string? action)
    {
        return !string.IsNullOrWhiteSpace(action) && All.Contains(action);
    }

    /// <summary>
    /// Converts a recommendation action enum value to the database label.
    /// </summary>
    public static string ToDatabaseValue(RecommendedAction action)
    {
        return action switch
        {
            RecommendedAction.NoAction => NoAction,
            RecommendedAction.Monitor => Monitor,
            RecommendedAction.RequestMoreInformation => RequestMoreInformation,
            RecommendedAction.ReviewDenial => ReviewDenial,
            RecommendedAction.FollowUpWithPayerProvider => FollowUpWithPayerProvider,
            RecommendedAction.PrepareDemandLetter => PrepareDemandLetter,
            RecommendedAction.EscalateForAttorneyReview => EscalateForAttorneyReview,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unsupported recommended action.")
        };
    }

    /// <summary>
    /// Parses a database label into the matching recommendation action enum value.
    /// </summary>
    public static bool TryParseDatabaseValue(string? value, out RecommendedAction action)
    {
        action = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        action = value switch
        {
            NoAction => RecommendedAction.NoAction,
            Monitor => RecommendedAction.Monitor,
            RequestMoreInformation => RecommendedAction.RequestMoreInformation,
            ReviewDenial => RecommendedAction.ReviewDenial,
            FollowUpWithPayerProvider => RecommendedAction.FollowUpWithPayerProvider,
            PrepareDemandLetter => RecommendedAction.PrepareDemandLetter,
            EscalateForAttorneyReview => RecommendedAction.EscalateForAttorneyReview,
            _ => default
        };

        return ToDatabaseValue(action) == value;
    }
}
